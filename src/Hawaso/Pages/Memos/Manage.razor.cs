using BlazorUtils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using VisualAcademy.Pages.Memos.Components;

namespace VisualAcademy.Pages.Memos;

public partial class Manage
{
    #region Parameters
    // 정수 형식의 Id 값을 받는 공통 속성 이름
    [Parameter]
    public int ParentId { get; set; } = 0;

    // 문자열 형식의 Id 값을 받는 공통 속성 이름 
    [Parameter]
    public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    // NavigationManager를 참조해서 사용하기에 Injector 접미사를 붙임 또는 _(언더스코어) 접두사 붙임 
    // public NavigationManager NavigationManager { get; set; } 형태가 기본 사용 방식임
    [Inject]
    public NavigationManager NavigationManagerInjector { get; set; }

    [Inject]
    public IJSRuntime JSRuntimeInjector { get; set; }

    // Repository 참조는 Reference 접미사를 붙여봄 
    [Inject]
    public IMemoRepository RepositoryReference { get; set; }

    [Inject]
    public IMemoFileStorageManager FileStorageManagerReference { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// 글쓰기 또는 수정하기 폼의 제목에 전달할 문자열(태그 포함 가능)
    /// </summary>
    public string EditorFormTitle { get; set; } = "CREATE";

    /// <summary>
    /// 현재 페이지에서 리스트로 사용되는 모델 리스트 
    /// </summary>
    //protected List<Memo> models = new List<Memo>();
    //protected List<Memo> Models = new();
    public List<Memo> Models { get; set; } = [];

    /// <summary>
    /// 현재 페이지에서 선택된 단일 데이터를 나타내는 모델 클래스 
    /// </summary>
    //protected Memo model = new Memo();
    public Memo Model { get; set; } = new();

    /// <summary>
    /// 페이저 설정
    /// </summary>
    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };
    #endregion

    #region Component References
    /// <summary>
    /// EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기
    /// </summary>
    //public Components.EditorForm EditorFormReference { get; set; }
    public ModalForm EditorFormReference { get; set; }

    /// <summary>
    /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기 
    /// </summary>
    public DeleteDialog DeleteDialogReference { get; set; } 
    #endregion

    #region Search
    private string searchQuery = "";

    protected async void Search(string query)
    {
        pager.PageIndex = 0; // Pager 컴포넌트 인덱스 초기화 후 검색

        this.searchQuery = query;

        await DisplayData();
    }
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Razor Page 또는 MVC에서 인증 정보를 Blazor Server로 전송하여 Blazor에서는 따로 인증된 사용자 정보를 읽어오지 않도록 설정
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
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, ParentKey);
            pager.RecordCount = articleSet.TotalCount;
            Models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, ParentId);
            pager.RecordCount = articleSet.TotalCount;
            Models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, parentIdentifier: 0);
            pager.RecordCount = articleSet.TotalCount;
            Models = articleSet.Items.ToList();
        }

        StateHasChanged(); // Refresh
    }

    /// <summary>
    /// 상세보기로 이동하는 링크
    /// </summary>
    protected void NameClick(long id) => NavigationManagerInjector.NavigateTo($"/Memos/Details/{id}");

    /// <summary>
    /// Pager 링크 버튼 클릭에 따른 리스트 내용 업데이트
    /// </summary>
    /// <param name="pageIndex">페이지 인덱스</param>
    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1; // 하위 호환성때문에 이 코드 유지 

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
        Model = new Memo(); // 모델 초기화
        this.Model.ParentId = Model.ParentId;
        this.Model.ParentKey = Model.ParentKey;

        Model.Name = UserName; // 로그인 사용자 이름을 기본으로 제공

        EditorFormReference.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 수정
    /// </summary>
    protected void EditBy(Memo model)
    {
        EditorFormTitle = "EDIT";
        this.Model = new Memo(); // 모델 초기화
        this.Model = model;
        //this.model.ParentId = ParentId;
        this.Model.ParentId = model.ParentId;
        //this.model.ParentKey = ParentKey; 
        this.Model.ParentKey = model.ParentKey;
        EditorFormReference.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 삭제
    /// </summary>
    protected void DeleteBy(Memo model)
    {
        this.Model = model;
        DeleteDialogReference.Show();
    }
    #endregion

    protected async void DownloadBy(Memo model)
    {
        if (!string.IsNullOrEmpty(model.FileName))
        {
            byte[] fileBytes = await FileStorageManagerReference.DownloadAsync(model.FileName, "Memos");
            if (fileBytes != null)
            {
                model.DownCount = model.DownCount + 1;
                await RepositoryReference.EditAsync(model);

                await FileUtil.SaveAs(JSRuntimeInjector, model.FileName, fileBytes);
            }
        }
    }

    /// <summary>
    /// 모델 초기화 및 모달 폼 닫기
    /// </summary>
    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();
        this.Model = new Memo();
        await DisplayData();
    }

    /// <summary>
    /// 삭제 모달 폼에서 현재 선택한 항목 삭제
    /// </summary>
    protected async void DeleteClick()
    {
        if (!string.IsNullOrEmpty(Model?.FileName))
        {
            // 첨부 파일 삭제 
            await FileStorageManagerReference.DeleteAsync(Model.FileName, "Memos");
        }

        await RepositoryReference.DeleteAsync(this.Model.Id);
        DeleteDialogReference.Hide();
        this.Model = new Memo(); // 선택했던 모델 초기화
        await DisplayData(); // 다시 로드
    }

    #region Toggle with Inline Dialog
    /// <summary>
    /// 인라인 폼을 띄울건지 여부 
    /// </summary>
    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        this.Model = new Memo();
    }

    /// <summary>
    /// 토글: Pinned
    /// </summary>
    protected async void ToggleClick()
    {
        this.Model.IsPinned = (this.Model?.IsPinned == true) ? false : true;

        // 변경된 내용 업데이트
        await RepositoryReference.UpdateAsync(this.Model);

        IsInlineDialogShow = false; // 표시 속성 초기화
        this.Model = new Memo(); // 선택한 모델 초기화 

        await DisplayData(); // 다시 로드
    }

    /// <summary>
    /// ToggleBy(PinnedBy)
    /// </summary>
    protected void ToggleBy(Memo model)
    {
        Model = model;
        IsInlineDialogShow = true;
    }
    #endregion

    #region Excel
    protected void DownloadExcelWithWebApi()
    {
        FileUtil.SaveAsExcel(JSRuntimeInjector, "/MemoDownload/ExcelDown");

        NavigationManagerInjector.NavigateTo($"/Memos"); // 다운로드 후 현재 페이지 다시 로드
    }

    protected void DownloadExcel()
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Memos");

            var tableBody = worksheet.Cells["A1:A1"].LoadFromCollection(
                (from m in Models select new { m.Created, m.Name, m.Title, m.DownCount, m.FileName })
                , true);

            var uploadCol = tableBody.Offset(1, 1, Models.Count, 1);

            // 그라데이션 효과 부여
            var rule = uploadCol.ConditionalFormatting.AddThreeColorScale();
            rule.LowValue.Color = Color.SkyBlue;
            rule.MiddleValue.Color = Color.White;
            rule.HighValue.Color = Color.Red;

            var header = worksheet.Cells["A1:E1"];
            worksheet.DefaultColWidth = 25;
            worksheet.Cells[2, 1, Models.Count + 1, 1].Style.Numberformat.Format = "yyyy MMM d DDD";
            tableBody.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            tableBody.Style.Fill.PatternType = ExcelFillStyle.Solid;
            tableBody.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
            tableBody.Style.Border.BorderAround(ExcelBorderStyle.Medium);
            header.Style.Font.Bold = true;
            header.Style.Font.Color.SetColor(Color.White);
            header.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

            FileUtil.SaveAs(JSRuntimeInjector, $"{DateTime.Now.ToString("yyyyMMddhhmmss")}_Memos.xlsx", package.GetAsByteArray());
        }
    }
    #endregion

    #region Sorting
    // 이 코드 부분은 Grid 컴포넌트에서 컬럼 정렬을 수행하기 위한 로직을 담고 있습니다.
    // .NET 7의 QuickGrid에서 사용하는 방식처럼 더 향상된 방식이 있지만,
    // 이 코드는 처음 Sorting 기능을 적용할 때 사용한 기본적인 구분 방식을 사용하고 있습니다.
    // sortOrder 변수는 현재 어떤 컬럼이 어떤 방식(오름차순, 내림차순)으로 정렬되어 있는지를 나타냅니다.
    private string sortOrder = "";

    // 아래의 각 메서드는 특정 컬럼에 대해 정렬을 수행합니다.
    // 각 메서드는 현재의 sortOrder 값을 검사하고, 이에 따라 적절하게 정렬 순서를 변경합니다.
    // 메서드가 호출될 때마다, sortOrder 값이 업데이트되며,
    // 이 값은 비동기적으로 데이터를 화면에 표시하는 DisplayData 메서드에 의해 사용됩니다.

    // 이 메서드는 "Create" 컬럼에 대한 정렬을 수행합니다.
    protected async void SortByCreate()
    {
        // sortOrder 값이 "Create"를 포함하고 있지 않으면, sortOrder를 초기화합니다.
        if (!sortOrder.Contains("Create"))
        {
            sortOrder = "";
        }

        // sortOrder 값이 비어있으면, sortOrder를 "Create"로 설정하여 오름차순 정렬하도록 합니다.
        if (sortOrder == "")
        {
            sortOrder = "Create";
        }
        // sortOrder 값이 "Create"이면, sortOrder를 "CreateDesc"로 설정하여 내림차순 정렬하도록 합니다.
        else if (sortOrder == "Create")
        {
            sortOrder = "CreateDesc";
        }
        // 그 외의 경우는 sortOrder를 초기화합니다.
        else
        {
            sortOrder = "";
        }

        // 데이터를 새로 고침하여 변경된 정렬 순서를 반영합니다.
        await DisplayData();
    }

    // 이 메서드는 "Name" 컬럼에 대한 정렬을 수행합니다.
    protected async void SortByName()
    {
        if (!sortOrder.Contains("Name"))
        {
            sortOrder = "";
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

    // 이 메서드는 "Title" 컬럼에 대한 정렬을 수행합니다.
    protected async void SortByTitle()
    {
        if (!sortOrder.Contains("Title"))
        {
            sortOrder = "";
        }

        if (sortOrder == "")
        {
            sortOrder = "Title";
        }
        else if (sortOrder == "Title")
        {
            sortOrder = "TitleDesc";
        }
        else
        {
            sortOrder = "";
        }

        await DisplayData();
    }
    #endregion

    #region Get UserId and UserName: Blazor에서 현재 로그인 사용자 이름 획득하기
    [Parameter]
    public string UserId { get; set; } = "";

    [Parameter]
    public string UserName { get; set; } = "";

    [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; }

    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; }

    private async Task GetUserIdAndUserName()
    {
        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity.IsAuthenticated)
        {
            var currentUser = await UserManagerRef.GetUserAsync(user);
            UserId = currentUser.Id;
            UserName = user.Identity.Name;
        }
        else
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }
    #endregion
}
