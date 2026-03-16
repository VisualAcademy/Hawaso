using Azunt.Components.Dialogs;
using BlazorUtils;
using Hawaso.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using VisualAcademy.Models.BannedTypes;

namespace VisualAcademy.Components.Pages.BannedTypes;

public partial class Manage
{
    #region Parameters
    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;

    [Parameter]
    public string UserId { get; set; } = string.Empty;

    [Parameter]
    public string UserName { get; set; } = string.Empty;
    #endregion

    #region Injectors
    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntimeInjector { get; set; } = default!;

    [Inject]
    public IBannedTypeRepository RepositoryReference { get; set; } = default!;

    [Inject]
    public UserManager<ApplicationUser> UserManagerRef { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = default!;
    #endregion

    #region Properties
    /// <summary>
    /// 글쓰기 또는 수정하기 폼의 제목에 전달할 문자열(태그 포함 가능)
    /// </summary>
    public string EditorFormTitle { get; set; } = "CREATE";

    /// <summary>
    /// 인라인 폼을 띄울건지 여부
    /// </summary>
    public bool IsInlineDialogShow { get; set; }
    #endregion

    /// <summary>
    /// EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기
    /// </summary>
    public Components.ModalForm? EditorFormReference { get; set; }

    /// <summary>
    /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기
    /// </summary>
    public DeleteDialog? DeleteDialogReference { get; set; }

    /// <summary>
    /// 현재 페이지에서 리스트로 사용되는 모델 리스트
    /// </summary>
    protected List<BannedTypeModel> models = new();

    /// <summary>
    /// 현재 페이지에서 선택된 단일 데이터를 나타내는 모델 클래스
    /// </summary>
    protected BannedTypeModel model = new();

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

    #region Fields
    private string searchQuery = string.Empty;
    private string sortOrder = string.Empty;
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(UserName))
        {
            await GetUserIdAndUserName();
        }

        await DisplayData();
    }
    #endregion

    private async Task DisplayData()
    {
        if (!string.IsNullOrEmpty(ParentKey))
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(
                pager.PageIndex,
                pager.PageSize,
                string.Empty,
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
                string.Empty,
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
                searchField: string.Empty,
                searchQuery: searchQuery,
                sortOrder: sortOrder,
                parentIdentifier: 0);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();
    }

    #region Event Handlers
    /// <summary>
    /// 글쓰기 모달 폼 띄우기
    /// </summary>
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        model = new BannedTypeModel();
        EditorFormReference?.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 수정
    /// </summary>
    protected void EditBy(BannedTypeModel selectedModel)
    {
        EditorFormTitle = "EDIT";
        model = selectedModel ?? new BannedTypeModel();
        EditorFormReference?.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 삭제
    /// </summary>
    protected void DeleteBy(BannedTypeModel selectedModel)
    {
        model = selectedModel ?? new BannedTypeModel();
        DeleteDialogReference?.Show();
    }
    #endregion

    /// <summary>
    /// 모델 초기화 및 모달 폼 닫기
    /// </summary>
    protected async void CreateOrEdit()
    {
        EditorFormReference?.Hide();
        model = new BannedTypeModel();

        await DisplayData();
    }

    /// <summary>
    /// 삭제 모달 폼에서 현재 선택한 항목 삭제
    /// </summary>
    protected async void DeleteClick()
    {
        await RepositoryReference.DeleteAsync(model.Id);
        DeleteDialogReference?.Hide();
        model = new BannedTypeModel();

        await DisplayData();
    }

    #region Toggle with Inline Dialog
    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        model = new BannedTypeModel();
    }

    /// <summary>
    /// 토글: Active
    /// </summary>
    protected async void ToggleClick()
    {
        model.Active = !model.Active;

        await RepositoryReference.UpdateAsync(model);

        IsInlineDialogShow = false;
        model = new BannedTypeModel();

        await DisplayData();
    }

    /// <summary>
    /// ToggleBy
    /// </summary>
    protected void ToggleBy(BannedTypeModel selectedModel)
    {
        model = selectedModel ?? new BannedTypeModel();
        IsInlineDialogShow = true;
    }
    #endregion

    #region Search
    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        pager.PageNumber = 1;
        searchQuery = query ?? string.Empty;

        await DisplayData();
    }
    #endregion

    #region Excel
    protected void DownloadExcelWithWebApi()
    {
        FileUtil.SaveAsExcel(JSRuntimeInjector, "/BannedTypeDownload/ExcelDown");
        Nav.NavigateTo("/BannedTypes");
    }

    protected void DownloadExcel()
    {
        //using (var package = new ExcelPackage())
        //{
        //    var worksheet = package.Workbook.Worksheets.Add("BannedTypes");
        //
        //    var tableBody = worksheet.Cells["B2:B2"].LoadFromCollection(
        //        (from m in models select new { m.Created, m.Name, m.Title, m.DownCount, m.FileName }),
        //        true);
        //
        //    var uploadCol = tableBody.Offset(1, 1, models.Count, 1);
        //
        //    var rule = uploadCol.ConditionalFormatting.AddThreeColorScale();
        //    rule.LowValue.Color = Color.SkyBlue;
        //    rule.MiddleValue.Color = Color.White;
        //    rule.HighValue.Color = Color.Red;
        //
        //    var header = worksheet.Cells["B2:F2"];
        //    worksheet.DefaultColWidth = 25;
        //    worksheet.Cells[3, 2, models.Count + 2, 2].Style.Numberformat.Format = "yyyy MMM d DDD";
        //    tableBody.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        //    tableBody.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //    tableBody.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
        //    tableBody.Style.Border.BorderAround(ExcelBorderStyle.Medium);
        //    header.Style.Font.Bold = true;
        //    header.Style.Font.Color.SetColor(Color.White);
        //    header.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
        //
        //    FileUtil.SaveAs(JSRuntimeInjector, $"{DateTime.Now:yyyyMMddhhmmss}_BannedTypes.xlsx", package.GetAsByteArray());
        //}
    }
    #endregion

    #region Sorting
    protected async void SortByName()
    {
        if (!sortOrder.Contains("Name", StringComparison.Ordinal))
        {
            sortOrder = string.Empty;
        }

        if (sortOrder == string.Empty)
        {
            sortOrder = "Name";
        }
        else if (sortOrder == "Name")
        {
            sortOrder = "NameDesc";
        }
        else
        {
            sortOrder = string.Empty;
        }

        await DisplayData();
    }
    #endregion

    #region Get UserId and UserName
    private async Task GetUserIdAndUserName()
    {
        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var currentUser = await UserManagerRef.GetUserAsync(user);

            if (currentUser is not null)
            {
                UserId = currentUser.Id ?? string.Empty;
                UserName = user.Identity?.Name ?? currentUser.UserName ?? "Anonymous";
            }
            else
            {
                UserId = string.Empty;
                UserName = user.Identity?.Name ?? "Anonymous";
            }
        }
        else
        {
            UserId = string.Empty;
            UserName = "Anonymous";
        }
    }
    #endregion
}