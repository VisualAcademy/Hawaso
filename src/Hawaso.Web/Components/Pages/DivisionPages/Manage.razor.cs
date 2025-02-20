using Hawaso.Web.Components.Pages.DivisionPages.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace Hawaso.Web.Components.Pages.DivisionPages;

public partial class Manage : ComponentBase
{
    /// <summary>
    /// Bootstrap 5 ��� ���� (�⺻��: true)
    /// </summary>
    [Parameter]
    public bool UseBootstrap5 { get; set; } = true;

    #region Parameters
    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    [Inject]
    public NavigationManager NavigationManagerInjector { get; set; }

    [Inject]
    public IJSRuntime JSRuntimeInjector { get; set; }

    [Inject]
    public IDivisionRepository RepositoryReference { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// �۾��� �Ǵ� �����ϱ� ���� ���� ������ ���ڿ�(�±� ���� ����)
    /// </summary>
    public string EditorFormTitle { get; set; } = "CREATE";
    #endregion

    /// <summary>
    /// EditorForm�� ���� ����: ��޷� �۾��� �Ǵ� �����ϱ�
    /// </summary>
    //public Components.EditorForm EditorFormReference { get; set; }
    public Components.ModalForm EditorFormReference { get; set; }

    /// <summary>
    /// DeleteDialog�� ���� ����: ��޷� �׸� �����ϱ� 
    /// </summary>
    public Components.DeleteDialog DeleteDialogReference { get; set; }

    /// <summary>
    /// ���� ���������� ����Ʈ�� ���Ǵ� �� ����Ʈ 
    /// </summary>
    protected List<DivisionModel> models = new List<DivisionModel>();

    /// <summary>
    /// ���� ���������� ���õ� ���� �����͸� ��Ÿ���� �� Ŭ���� 
    /// </summary>
    protected DivisionModel model = new DivisionModel();

    /// <summary>
    /// ������ ����
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
    /// ������ �ʱ�ȭ �̺�Ʈ ó����
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
        // ParentKey�� ParentId�� ����ϴ� ������ Ư�� �θ��� Details ���������� ����Ʈ�� ǥ���ϱ� ����
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
    /// �۾��� ��� �� ���� 
    /// </summary>
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        this.model = new DivisionModel(); // �� �ʱ�ȭ
        EditorFormReference.Show();
    }

    /// <summary>
    /// ������ ����: ��� ������ ���� �׸� ����
    /// </summary>
    protected void EditBy(DivisionModel model)
    {
        EditorFormTitle = "EDIT";
        this.model = new DivisionModel(); // �� �ʱ�ȭ
        this.model = model;
        EditorFormReference.Show();
    }

    /// <summary>
    /// ������ ����: ��� ������ ���� �׸� ����
    /// </summary>
    protected void DeleteBy(DivisionModel model)
    {
        this.model = model;
        DeleteDialogReference.Show();
    }
    #endregion

    /// <summary>
    /// �� �ʱ�ȭ �� ��� �� �ݱ�
    /// </summary>
    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();
        this.model = null;
        this.model = new DivisionModel();

        await DisplayData();
    }

    /// <summary>
    /// ���� ��� ������ ���� ������ �׸� ����
    /// </summary>
    protected async void DeleteClick()
    {
        await RepositoryReference.DeleteAsync(this.model.Id);
        DeleteDialogReference.Hide();
        this.model = new DivisionModel(); // �����ߴ� �� �ʱ�ȭ
        await DisplayData(); // �ٽ� �ε�
    }

    #region Toggle with Inline Dialog
    /// <summary>
    /// �ζ��� ���� ������ ���� 
    /// </summary>
    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        this.model = new DivisionModel();
    }

    /// <summary>
    /// ���: Pinned
    /// </summary>
    protected async void ToggleClick()
    {
        model.Active = !model.Active;

        // ����� ���� ������Ʈ
        await RepositoryReference.UpdateAsync(this.model);

        IsInlineDialogShow = false; // ǥ�� �Ӽ� �ʱ�ȭ
        this.model = new DivisionModel(); // ������ �� �ʱ�ȭ 

        await DisplayData(); // �ٽ� �ε�
    }

    /// <summary>
    /// ToggleBy(PinnedBy)
    /// </summary>
    protected void ToggleBy(DivisionModel model)
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

        NavigationManagerInjector.NavigateTo($"/Divisions"); // �ٿ�ε� �� ���� ������ �ٽ� �ε�
    }

    protected void DownloadExcel()
    {
        //using (var package = new ExcelPackage())
        //{
        //    var worksheet = package.Workbook.Worksheets.Add("Divisions");

        //    var tableBody = worksheet.Cells["B2:B2"].LoadFromCollection(
        //        (from m in models select new { m.Created, m.Name, m.Title, m.DownCount, m.FileName })
        //        , true);

        //    var uploadCol = tableBody.Offset(1, 1, models.Count, 1);

        //    // �׶��̼� ȿ�� �ο�
        //    var rule = uploadCol.ConditionalFormatting.AddThreeColorScale();
        //    rule.LowValue.Color = Color.SkyBlue;
        //    rule.MiddleValue.Color = Color.White;
        //    rule.HighValue.Color = Color.Red;

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

        //    FileUtil.SaveAs(JSRuntimeInjector, $"{DateTime.Now.ToString("yyyyMMddhhmmss")}_Divisions.xlsx", package.GetAsByteArray());
        //}
    }
    #endregion

    #region Sorting
    private string sortOrder = "";

    protected async void SortByName()
    {
        if (!sortOrder.Contains("Name"))
        {
            sortOrder = ""; // �ٸ� ���� �����ϰ� �־��ٸ�, �ٽ� �ʱ�ȭ
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

    //[Inject] public UserManager<VisualAcademy.Data.ApplicationUser> UserManagerRef { get; set; }

    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; }

    private async Task GetUserIdAndUserName()
    {
        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity.IsAuthenticated)
        {
            //var currentUser = await UserManagerRef.GetUserAsync(user);
            //UserId = currentUser.Id;
            //UserName = user.Identity.Name;
        }
        else
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }
    #endregion
}