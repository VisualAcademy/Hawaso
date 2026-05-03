using Hawaso.Web.Components.Pages.DivisionPages.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Hawaso.Web.Components.Pages.DivisionPages;

public partial class Manage : ComponentBase
{
    /// <summary>
    /// Bootstrap 5 사용 여부 (기본값: true)
    /// </summary>
    [Parameter]
    public bool UseBootstrap5 { get; set; } = true;

    #region Parameters

    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = "";

    [Parameter]
    public string UserId { get; set; } = "";

    [Parameter]
    public string UserName { get; set; } = "";

    #endregion

    #region Injectors

    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntimeInjector { get; set; } = default!;

    [Inject]
    public IDivisionRepository RepositoryReference { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = default!;

    #endregion

    #region Properties

    /// <summary>
    /// 글쓰기 또는 수정하기 폼의 제목에 전달할 문자열(태그 포함 가능)
    /// </summary>
    public string EditorFormTitle { get; set; } = "CREATE";

    /// <summary>
    /// EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기
    /// </summary>
    public Components.ModalForm EditorFormReference { get; set; } = default!;

    /// <summary>
    /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기
    /// </summary>
    public Components.DeleteDialog DeleteDialogReference { get; set; } = default!;

    /// <summary>
    /// 현재 페이지에서 리스트로 사용되는 모델 리스트
    /// </summary>
    protected List<DivisionModel> models = new();

    /// <summary>
    /// 현재 페이지에서 선택된 단일 데이터를 나타내는 모델 클래스
    /// </summary>
    protected DivisionModel model = new();

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

    /// <summary>
    /// 인라인 폼을 띄울건지 여부
    /// </summary>
    public bool IsInlineDialogShow { get; set; } = false;

    protected string sortOrder = "";

    private string searchQuery = "";

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(UserId) && string.IsNullOrWhiteSpace(UserName))
        {
            await GetUserIdAndUserName();
        }

        await DisplayData();
    }

    #endregion

    #region Data

    private async Task DisplayData()
    {
        // ParentKey와 ParentId를 사용하는 목적은 특정 부모의 Details 페이지에서 리스트로 표현하기 위함
        if (!string.IsNullOrWhiteSpace(ParentKey))
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(
                pager.PageIndex,
                pager.PageSize,
                searchField: "",
                searchQuery,
                sortOrder,
                ParentKey);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
                pager.PageIndex,
                pager.PageSize,
                searchField: "",
                searchQuery,
                sortOrder,
                ParentId);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
                pager.PageIndex,
                pager.PageSize,
                searchField: "",
                searchQuery,
                sortOrder,
                parentIdentifier: 0);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }

    protected async Task PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 글쓰기 모달 폼 띄우기
    /// </summary>
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        model = new DivisionModel();

        EditorFormReference.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 수정
    /// </summary>
    protected void EditBy(DivisionModel selectedModel)
    {
        EditorFormTitle = "EDIT";
        model = selectedModel;

        EditorFormReference.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 삭제
    /// </summary>
    protected void DeleteBy(DivisionModel selectedModel)
    {
        model = selectedModel;

        DeleteDialogReference.Show();
    }

    /// <summary>
    /// ModalForm의 CreateCallback/EditCallback에서 호출되는 void 콜백 메서드
    /// </summary>
    protected async void CreateOrEditCallback()
    {
        await CreateOrEditAsync();
    }

    /// <summary>
    /// 실제 생성/수정 후처리 비동기 로직
    /// </summary>
    private async Task CreateOrEditAsync()
    {
        EditorFormReference.Hide();
        model = new DivisionModel();

        await DisplayData();
    }

    /// <summary>
    /// DeleteDialog의 OnClickCallback에서 호출되는 void 콜백 메서드
    /// </summary>
    protected async void DeleteClickCallback()
    {
        await DeleteClickAsync();
    }

    /// <summary>
    /// 실제 삭제 비동기 로직
    /// </summary>
    private async Task DeleteClickAsync()
    {
        await RepositoryReference.DeleteAsync(model.Id);

        DeleteDialogReference.Hide();
        model = new DivisionModel();

        await DisplayData();
    }

    #endregion

    #region Toggle with Inline Dialog

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        model = new DivisionModel();
    }

    /// <summary>
    /// 토글: Active
    /// </summary>
    protected async Task ToggleClick()
    {
        model.Active = !model.Active;

        await RepositoryReference.UpdateAsync(model);

        IsInlineDialogShow = false;
        model = new DivisionModel();

        await DisplayData();
    }

    /// <summary>
    /// ToggleBy
    /// </summary>
    protected void ToggleBy(DivisionModel selectedModel)
    {
        model = selectedModel;
        IsInlineDialogShow = true;
    }

    #endregion

    #region Search

    protected async Task Search(string query)
    {
        pager.PageIndex = 0;
        pager.PageNumber = 1;

        searchQuery = query;

        await DisplayData();
    }

    #endregion

    #region Excel

    protected void DownloadExcelWithWebApi()
    {
        // FileUtil.SaveAsExcel(JSRuntimeInjector, "/DivisionDownload/ExcelDown");

        Nav.NavigateTo("/DivisionDownload/ExcelDown", forceLoad: true);
    }

    protected void DownloadExcel()
    {
        // using (var package = new ExcelPackage())
        // {
        //     var worksheet = package.Workbook.Worksheets.Add("Divisions");
        //
        //     var tableBody = worksheet.Cells["B2:B2"].LoadFromCollection(
        //         (from m in models select new { m.Created, m.Name, m.Title, m.DownCount, m.FileName }),
        //         true);
        //
        //     var uploadCol = tableBody.Offset(1, 1, models.Count, 1);
        //
        //     var rule = uploadCol.ConditionalFormatting.AddThreeColorScale();
        //     rule.LowValue.Color = Color.SkyBlue;
        //     rule.MiddleValue.Color = Color.White;
        //     rule.HighValue.Color = Color.Red;
        //
        //     var header = worksheet.Cells["B2:F2"];
        //     worksheet.DefaultColWidth = 25;
        //     worksheet.Cells[3, 2, models.Count + 2, 2].Style.Numberformat.Format = "yyyy MMM d DDD";
        //     tableBody.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        //     tableBody.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //     tableBody.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
        //     tableBody.Style.Border.BorderAround(ExcelBorderStyle.Medium);
        //     header.Style.Font.Bold = true;
        //     header.Style.Font.Color.SetColor(Color.White);
        //     header.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
        //
        //     FileUtil.SaveAs(
        //         JSRuntimeInjector,
        //         $"{DateTime.Now:yyyyMMddhhmmss}_Divisions.xlsx",
        //         package.GetAsByteArray());
        // }
    }

    #endregion

    #region Sorting

    protected async Task SortByName()
    {
        if (!sortOrder.Contains("Name"))
        {
            sortOrder = "";
        }

        sortOrder = sortOrder switch
        {
            "" => "Name",
            "Name" => "NameDesc",
            _ => ""
        };

        await DisplayData();
    }

    #endregion

    #region Get UserId and UserName

    private async Task GetUserIdAndUserName()
    {
        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // var currentUser = await UserManagerRef.GetUserAsync(user);
            // UserId = currentUser.Id;

            UserName = user.Identity.Name ?? "";
        }
        else
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }

    #endregion
}