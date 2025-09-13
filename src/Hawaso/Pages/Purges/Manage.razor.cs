using BlazorUtils;
// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Globalization;

namespace VisualAcademy.Pages.Purges;

public partial class Manage
{
    #region Parameters
    [Parameter] public int ParentId { get; set; } = 0;
    [Parameter] public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    [Inject] public NavigationManager Nav { get; set; }
    [Inject] public IJSRuntime JSRuntimeInjector { get; set; }
    [Inject] public IPurgeRepository RepositoryReference { get; set; }
    [Inject] public IPurgeFileStorageManager FileStorageManagerReference { get; set; }
    #endregion

    #region Properties
    public string EditorFormTitle { get; set; } = "CREATE";
    public Components.DeleteDialog DeleteDialogReference { get; set; }
    protected List<Purge> models = new();
    protected Purge model = new();

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };
    #endregion

    #region Lifecycle
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
        if (!string.IsNullOrEmpty(ParentKey))
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(
                pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, ParentKey);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
                pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, ParentId);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
                pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, parentIdentifier: 0);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }

    protected void NameClick(long id) => Nav.NavigateTo($"/Purges/Details/{id}");

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;
        await DisplayData();
        StateHasChanged();
    }

    #region Event Handlers
    protected void ShowEditorForm()
    {
        // 현재 템플릿에서는 모달 사용 안 함(주석 유지)
    }

    protected void EditBy(Purge model)
    {
        // 현재 템플릿에서는 모달 사용 안 함(주석 유지)
    }

    protected void DeleteBy(Purge model)
    {
        this.model = model;
        DeleteDialogReference.Show();
    }
    #endregion

    protected async void DownloadBy(Purge model)
    {
        if (!string.IsNullOrEmpty(model.FileName))
        {
            var fileBytes = await FileStorageManagerReference.DownloadAsync(model.FileName, "Purges");
            if (fileBytes != null)
            {
                // DownCount: int? → null-safe 증가
                model.DownCount = (model.DownCount ?? 0) + 1;
                await RepositoryReference.EditAsync(model);

                await FileUtil.SaveAs(JSRuntimeInjector, model.FileName, fileBytes);
            }
        }
    }

    protected async void DeleteClick()
    {
        if (!string.IsNullOrEmpty(model?.FileName))
        {
            await FileStorageManagerReference.DeleteAsync(model.FileName, "Purges");
        }

        await RepositoryReference.DeleteAsync(this.model.Id);
        DeleteDialogReference.Hide();
        this.model = new Purge();
        await DisplayData();
    }

    #region Toggle with Inline Dialog
    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        this.model = new Purge();
    }

    protected async void ToggleClick()
    {
        this.model.IsPinned = (this.model?.IsPinned == true) ? false : true;

        await RepositoryReference.UpdateAsync(this.model);

        IsInlineDialogShow = false;
        this.model = new Purge();

        await DisplayData();
    }

    protected void ToggleBy(Purge model)
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

    #region Excel (OpenXML, 컴포넌트 내 직접 생성)
    // 서버로 위임하지 않고(OpenXML로) 엑셀을 메모리에서 생성하여 바로 다운로드
    protected void DownloadExcel()
    {
        if (models.Count == 0)
        {
            // 비어있어도 빈 파일을 만들 수 있지만, UX상 여기선 그냥 리턴
            return;
        }

        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                var wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();

                var wsPart = wbPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                wsPart.Worksheet = new Worksheet(sheetData);

                var sheets = wbPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = wbPart.GetIdOfPart(wsPart),
                    SheetId = 1U,
                    Name = "Purges"
                });

                // Header
                uint headerRowIndex = 1;
                var headerRow = new Row { RowIndex = headerRowIndex };
                sheetData.Append(headerRow);

                // 필요한 컬럼: Created, Name, Title, DownCount, FileName
                string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.Append(TextCell(CellRef(i + 1, (int)headerRowIndex), headers[i]));
                }

                // Data rows
                uint rowIndex = 2;
                foreach (var m in models)
                {
                    var row = new Row { RowIndex = rowIndex };
                    sheetData.Append(row);

                    var createdStr = m.Created.HasValue
                        ? m.Created.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                        : string.Empty;

                    // DownCount: int? → 문자열
                    var downCountStr = (m.DownCount ?? 0).ToString(CultureInfo.InvariantCulture);

                    var values = new[]
                    {
                        createdStr,
                        m.Name ?? string.Empty,
                        m.Title ?? string.Empty,
                        downCountStr,
                        m.FileName ?? string.Empty
                    };

                    for (int i = 0; i < values.Length; i++)
                    {
                        row.Append(TextCell(CellRef(i + 1, (int)rowIndex), values[i]));
                    }

                    rowIndex++;
                }

                wsPart.Worksheet.Save();
                wbPart.Workbook.Save();
            }

            bytes = ms.ToArray();
        }

        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Purges.xlsx";
        FileUtil.SaveAs(JSRuntimeInjector,
            fileName,
            bytes);
    }

    // === OpenXML helper ===
    private static Cell TextCell(string cellRef, string text) => new Cell
    {
        CellReference = cellRef,
        DataType = CellValues.String,
        CellValue = new CellValue(text ?? string.Empty)
    };

    private static string CellRef(int col1Based, int row) => $"{ColName(col1Based)}{row}";

    private static string ColName(int idx)
    {
        // 1 -> A, 2 -> B, ... 26 -> Z, 27 -> AA
        var dividend = idx;
        string col = string.Empty;
        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            col = (char)('A' + modulo) + col;
            dividend = (dividend - modulo) / 26;
        }
        return col;
    }
    #endregion

    #region Sorting
    private string sortOrder = "";

    protected async void SortByCreate()
    {
        if (!sortOrder.Contains("Create")) sortOrder = "";
        sortOrder = sortOrder switch
        {
            "" => "Create",
            "Create" => "CreateDesc",
            _ => ""
        };
        await DisplayData();
    }

    protected async void SortByName()
    {
        if (!sortOrder.Contains("Name")) sortOrder = "";
        sortOrder = sortOrder switch
        {
            "" => "Name",
            "Name" => "NameDesc",
            _ => ""
        };
        await DisplayData();
    }

    protected async void SortByTitle()
    {
        if (!sortOrder.Contains("Title")) sortOrder = "";
        sortOrder = sortOrder switch
        {
            "" => "Title",
            "Title" => "TitleDesc",
            _ => ""
        };
        await DisplayData();
    }
    #endregion

    #region Get UserId and UserName
    [Parameter] public string UserId { get; set; } = "";
    [Parameter] public string UserName { get; set; } = "";

    [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; }
    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; }

    private async Task GetUserIdAndUserName()
    {
        // 널-안전 가드
        if (AuthenticationStateProviderRef is null || UserManagerRef is null)
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
                UserId = currentUser.Id ?? "";
                UserName = user.Identity?.Name ?? currentUser.UserName ?? "Anonymous";
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
