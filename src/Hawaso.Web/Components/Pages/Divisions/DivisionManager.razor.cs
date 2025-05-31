using Azunt.Components.Dialogs;
using Azunt.DivisionManagement;
using Azunt.Web.Pages.Divisions.Components;
using Hawaso.Web.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace Azunt.Web.Components.Pages.Divisions;

public partial class DivisionManager : ComponentBase
{
    public bool SimpleMode { get; set; } = false;

    #region Parameters
    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    [Inject]
    public NavigationManager NavigationManagerInjector { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntimeInjector { get; set; } = null!;

    [Inject]
    public IDivisionRepository RepositoryReference { get; set; } = null!;

    [Inject]
    public IConfiguration Configuration { get; set; } = null!;

    [Inject]
    public DivisionDbContextFactory DbContextFactory { get; set; } = null!;
    #endregion

    #region Properties
    /// <summary>
    /// 글쓰기 또는 수정하기 폼의 제목에 전달할 문자열(태그 포함 가능)
    /// </summary>
    public string EditorFormTitle { get; set; } = "CREATE";
    #endregion

    /// <summary>
    /// EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기
    /// </summary>
    //public Components.EditorForm EditorFormReference { get; set; }
    public ModalForm EditorFormReference { get; set; } = null!; // null이 아닌 ModalForm으로 초기화

    /// <summary>
    /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기 
    /// </summary>
    public DeleteDialog DeleteDialogReference { get; set; } = null!;

    /// <summary>
    /// 현재 페이지에서 리스트로 사용되는 모델 리스트 
    /// </summary>
    protected List<Division> models = new List<Division>();

    /// <summary>
    /// 현재 페이지에서 선택된 단일 데이터를 나타내는 모델 클래스 
    /// </summary>
    protected Division model = new Division();

    /// <summary>
    /// 페이저 설정
    /// </summary>
    protected DulPager.DulPagerBase pager = new DulPager.DulPagerBase()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    #region Lifecycle Methods
    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (UserId == "" && UserName == "")
        {
            await GetUserIdAndUserName();
        }

        await DisplayData();
    }
    #endregion

    private async Task DisplayData()
    {
        // ParentKey와 ParentId를 사용하는 목적은 특정 부모의 Details 페이지에서 리스트로 표현하기 위함
        if (ParentKey != "")
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, ParentKey);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, ParentId);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, parentIdentifier: 0);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged(); // Refresh
    }

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();

        StateHasChanged();
    }

    #region Event Handlers
    /// <summary>
    /// 글쓰기 모달 폼 띄우기 
    /// </summary>
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        this.model = new Division(); // 모델 초기화
        EditorFormReference.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 수정
    /// </summary>
    protected void EditBy(Division model)
    {
        EditorFormTitle = "EDIT";
        this.model = new Division(); // 모델 초기화
        this.model = model;
        EditorFormReference.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 삭제
    /// </summary>
    protected void DeleteBy(Division model)
    {
        this.model = model;
        DeleteDialogReference.Show();
    }
    #endregion

    /// <summary>
    /// 모델 초기화 및 모달 폼 닫기
    /// </summary>
    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide(); // 모달 먼저 닫고

        await Task.Delay(50); // 아주 짧게 대기 (서버-클라이언트 싱크 맞추기)

        this.model = new Division(); // 초기화

        await DisplayData(); // 데이터 다시 로드
    }

    /// <summary>
    /// 삭제 모달 폼에서 현재 선택한 항목 삭제
    /// </summary>
    protected async void DeleteClickOld()
    {
        await RepositoryReference.DeleteAsync(this.model.Id);
        DeleteDialogReference.Hide();
        this.model = new Division(); // 선택했던 모델 초기화
        await DisplayData(); // 다시 로드
    }

    protected async void DeleteClick()
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        await RepositoryReference.DeleteAsync(this.model.Id, connectionString);
        DeleteDialogReference.Hide();
        this.model = new Division();
        await DisplayData();
    }

    #region Toggle with Inline Dialog
    /// <summary>
    /// 인라인 폼을 띄울건지 여부 
    /// </summary>
    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        this.model = new Division();
    }

    /// <summary>
    /// 토글: Pinned
    /// </summary>
    protected async void ToggleClickOld()
    {
        model.Active = !model.Active;

        // 변경된 내용 업데이트
        await RepositoryReference.UpdateAsync(this.model);

        IsInlineDialogShow = false; // 표시 속성 초기화
        this.model = new Division(); // 선택한 모델 초기화 

        await DisplayData(); // 다시 로드
    }
    protected async void ToggleClick()
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // 예외를 직접 던지거나, 기본 동작을 선택
            throw new InvalidOperationException("DefaultConnection is not configured properly.");
        }

        await using var context = DbContextFactory.CreateDbContext(connectionString);

        model.Active = !model.Active;

        context.Divisions.Update(model);
        await context.SaveChangesAsync();

        IsInlineDialogShow = false;
        this.model = new Division();

        await DisplayData();
    }

    /// <summary>
    /// ToggleBy(PinnedBy)
    /// </summary>
    protected void ToggleBy(Division model)
    {
        this.model = model;
        IsInlineDialogShow = true;
    }
    #endregion

    #region Search
    private string searchQuery = "";

    protected async void Search(string query)
    {
        pager.PageIndex = 0;

        this.searchQuery = query;

        await DisplayData();
    }
    #endregion

    #region Excel
    protected void DownloadExcelWithWebApi()
    {
        //FileUtil.SaveAsExcel(JSRuntimeInjector, "/DivisionDownload/ExcelDown");

        NavigationManagerInjector.NavigateTo($"/Divisions"); // 다운로드 후 현재 페이지 다시 로드
    }
    #endregion

    #region Sorting
    private string sortOrder = "";

    protected async void SortByName()
    {
        if (!sortOrder.Contains("Name"))
        {
            sortOrder = ""; // 다른 열을 정렬하고 있었다면, 다시 초기화
        }

        if (sortOrder == "")
        {
            sortOrder = "Name";
        }
        else if (sortOrder == "Name")
        {
            sortOrder = "NameDesc";
        }
        else
        {
            sortOrder = "";
        }

        await DisplayData();
    }
    #endregion

    #region Get UserId and UserName
    [Parameter]
    public string UserId { get; set; } = "";

    [Parameter]
    public string UserName { get; set; } = "";

    [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; } = null!;

    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = null!;

    private async Task GetUserIdAndUserName()
    {
        if (AuthenticationStateProviderRef == null || UserManagerRef == null)
        {
            UserId = "";
            UserName = "Anonymous";
            return;
        }

        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var currentUser = await UserManagerRef.GetUserAsync(user);
            if (currentUser != null)
            {
                UserId = currentUser.Id;
                UserName = user.Identity?.Name ?? "Anonymous";
            }
            else
            {
                UserId = "";
                UserName = "Anonymous";
            }
        }
        else
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }
    #endregion
}