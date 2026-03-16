using BlazorUtils;
using Hawaso.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisualAcademy.Models.Departments;

namespace VisualAcademy.Pages.Departments;

public partial class Manage
{
    #region Parameters
    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;
    #endregion

    #region Injectors
    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntimeInjector { get; set; } = default!;

    [Inject]
    public IDepartmentRepository RepositoryReference { get; set; } = default!;
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
    public Components.ModalForm? EditorFormReference { get; set; }

    /// <summary>
    /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기 
    /// </summary>
    public Components.DeleteDialog? DeleteDialogReference { get; set; }

    /// <summary>
    /// 현재 페이지에서 리스트로 사용되는 모델 리스트 
    /// </summary>
    protected List<DepartmentModel> models = new();

    /// <summary>
    /// 현재 페이지에서 선택된 단일 데이터를 나타내는 모델 클래스 
    /// </summary>
    protected DepartmentModel model = new();

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
        // ParentKey와 ParentId를 사용하는 목적은 특정 부모의 Details 페이지에서 리스트로 표현하기 위함
        if (!string.IsNullOrEmpty(ParentKey))
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(
                pager.PageIndex, pager.PageSize, "", searchQuery, sortOrder, ParentKey);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
                pager.PageIndex, pager.PageSize, "", searchQuery, sortOrder, ParentId);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
                pager.PageIndex, pager.PageSize, searchField: "", searchQuery, sortOrder, parentIdentifier: 0);

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

    #region Event Handlers
    /// <summary>
    /// 글쓰기 모달 폼 띄우기 
    /// </summary>
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        model = new DepartmentModel();
        EditorFormReference?.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 수정
    /// </summary>
    protected void EditBy(DepartmentModel model)
    {
        EditorFormTitle = "EDIT";
        this.model = model ?? new DepartmentModel();
        EditorFormReference?.Show();
    }

    /// <summary>
    /// 관리자 전용: 모달 폼으로 선택 항목 삭제
    /// </summary>
    protected void DeleteBy(DepartmentModel model)
    {
        this.model = model ?? new DepartmentModel();
        DeleteDialogReference?.Show();
    }
    #endregion

    /// <summary>
    /// 모델 초기화 및 모달 폼 닫기
    /// </summary>
    protected async Task CreateOrEdit()
    {
        EditorFormReference?.Hide();
        model = new DepartmentModel();

        await DisplayData();
    }

    /// <summary>
    /// 삭제 모달 폼에서 현재 선택한 항목 삭제
    /// </summary>
    protected async Task DeleteClick()
    {
        await RepositoryReference.DeleteAsync(model.Id);
        DeleteDialogReference?.Hide();
        model = new DepartmentModel();
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
        model = new DepartmentModel();
    }

    /// <summary>
    /// 토글: Pinned
    /// </summary>
    protected async Task ToggleClick()
    {
        model.Active = !model.Active;

        // 변경된 내용 업데이트
        await RepositoryReference.UpdateAsync(model);

        IsInlineDialogShow = false;
        model = new DepartmentModel();

        await DisplayData();
    }

    /// <summary>
    /// ToggleBy(PinnedBy)
    /// </summary>
    protected void ToggleBy(DepartmentModel model)
    {
        this.model = model ?? new DepartmentModel();
        IsInlineDialogShow = true;
    }
    #endregion

    #region Search
    private string searchQuery = string.Empty;

    protected async Task Search(string query)
    {
        pager.PageIndex = 0;
        searchQuery = query ?? string.Empty;

        await DisplayData();
    }
    #endregion

    #region Excel
    protected void DownloadExcelWithWebApi()
    {
        FileUtil.SaveAsExcel(JSRuntimeInjector, "/DepartmentDownload/ExcelDown");
        Nav.NavigateTo("/Departments");
    }

    protected void DownloadExcel()
    {
        //using (var package = new ExcelPackage())
        //{
        //    var worksheet = package.Workbook.Worksheets.Add("Departments");
        //
        //    var tableBody = worksheet.Cells["B2:B2"].LoadFromCollection(
        //        (from m in models select new { m.Created, m.Name, m.Title, m.DownCount, m.FileName })
        //        , true);
        //
        //    var uploadCol = tableBody.Offset(1, 1, models.Count, 1);
        //
        //    // 그라데이션 효과 부여
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
        //    FileUtil.SaveAs(JSRuntimeInjector, $"{DateTime.Now:yyyyMMddhhmmss}_Departments.xlsx", package.GetAsByteArray());
        //}
    }
    #endregion

    #region Sorting
    private string sortOrder = string.Empty;

    protected async Task SortByName()
    {
        if (!sortOrder.Contains("Name"))
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
    [Parameter]
    public string UserId { get; set; } = string.Empty;

    [Parameter]
    public string UserName { get; set; } = string.Empty;

    [Inject]
    public UserManager<ApplicationUser> UserManagerRef { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = default!;

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