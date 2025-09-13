using BlazorUtils;
using Hawaso.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Globalization;
using VisualAcademy.Pages.Memos.Components;
using System.IO;
using System.Linq;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace VisualAcademy.Pages.Memos;

public partial class Manage
{
    #region Parameters
    [Parameter] public int ParentId { get; set; } = 0;
    [Parameter] public string ParentKey { get; set; } = "";
    [Parameter] public bool IsReadOnly { get; set; } = false;
    #endregion

    #region Injectors
    [Inject] public NavigationManager Nav { get; set; }
    [Inject] public IJSRuntime JSRuntimeInjector { get; set; }
    [Inject] public IMemoRepository RepositoryReference { get; set; }
    [Inject] public IMemoFileStorageManager FileStorageManagerReference { get; set; }
    #endregion

    #region Properties
    public string EditorFormTitle { get; set; } = "CREATE";
    public List<Memo> Models { get; set; } = [];
    public Memo Model { get; set; } = new();

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };
    #endregion

    #region Component References
    public ModalForm EditorFormReference { get; set; }
    public DeleteDialog DeleteDialogReference { get; set; }
    #endregion

    #region Search
    private string searchQuery = "";

    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        searchQuery = query;
        await DisplayData();
    }
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        if (UserId == "" && UserName == "")
        {
            await GetUserIdAndUserName();
        }
        await DisplayData();
    }
    #endregion

    #region DisplayData()
    private async Task DisplayData()
    {
        if (ParentKey != "")
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(pager.PageIndex, pager.PageSize, searchField: "", searchQuery, sortOrder, ParentKey);
            pager.RecordCount = articleSet.TotalCount;
            Models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", searchQuery, sortOrder, ParentId);
            pager.RecordCount = articleSet.TotalCount;
            Models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", searchQuery, sortOrder, parentIdentifier: 0);
            pager.RecordCount = articleSet.TotalCount;
            Models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }
    #endregion

    #region Navigation
    protected void NameClick(long id) => Nav.NavigateTo($"/Memos/Details/{id}");
    #endregion

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;
        await DisplayData();
        StateHasChanged();
    }

    #region Editor + Delete
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        Model = new Memo
        {
            ParentId = ParentId,
            ParentKey = ParentKey,
            Name = UserName
        };
        EditorFormReference.Show();
    }

    protected void EditBy(Memo model)
    {
        EditorFormTitle = "EDIT";
        Model = new Memo();
        Model = model;
        Model.ParentId = model.ParentId;
        Model.ParentKey = model.ParentKey;
        EditorFormReference.Show();
    }

    protected void DeleteBy(Memo model)
    {
        Model = model;
        DeleteDialogReference.Show();
    }

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

    protected async void CreateOrEditOld()
    {
        EditorFormReference.Hide();
        Model = new Memo();
        await DisplayData();
    }

    protected void CreateOrEdit()
    {
        EditorFormReference.Hide();
        Nav.NavigateTo(Nav.Uri, forceLoad: true);
    }

    protected async void DeleteClick()
    {
        if (!string.IsNullOrEmpty(Model?.FileName))
        {
            await FileStorageManagerReference.DeleteAsync(Model.FileName, "Memos");
        }

        await RepositoryReference.DeleteAsync(Model.Id);
        DeleteDialogReference.Hide();
        Model = new Memo();
        await DisplayData();
    }
    #endregion

    #region Toggle with Inline Dialog
    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        Model = new Memo();
    }

    protected async void ToggleClick()
    {
        Model.IsPinned = (Model?.IsPinned == true) ? false : true;
        await RepositoryReference.UpdateAsync(Model);
        IsInlineDialogShow = false;
        Model = new Memo();
        await DisplayData();
    }

    protected void ToggleBy(Memo model)
    {
        Model = model;
        IsInlineDialogShow = true;
    }
    #endregion

    #region Excel (EPPlus 제거, OpenXML로 대체)
    protected void DownloadExcelWithWebApi()
    {
        FileUtil.SaveAsExcel(JSRuntimeInjector, "/MemoDownload/ExcelDown");
        Nav.NavigateTo($"/Memos");
    }

    protected void DownloadExcel()
    {
        var rows = Models ?? [];
        if (rows.Count == 0)
        {
            // 데이터 없을 때는 그냥 리턴 (필요 시 알림 표출)
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
                    Name = "Memos"
                });

                // Header
                uint headerRowIndex = 1;
                var header = new Row { RowIndex = headerRowIndex };
                sheetData.Append(header);

                string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
                for (int i = 0; i < headers.Length; i++)
                {
                    header.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                }

                // Data
                uint rowIndex = 2;
                foreach (var m in rows)
                {
                    var row = new Row { RowIndex = rowIndex };
                    sheetData.Append(row);

                    var createdStr = m.Created?.ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture) ?? string.Empty;

                    var values = new[]
                    {
                        createdStr,
                        m.Name ?? string.Empty,
                        m.Title ?? string.Empty,
                        (m.DownCount ?? 0).ToString(CultureInfo.InvariantCulture),
                        m.FileName ?? string.Empty
                    };

                    for (int i = 0; i < values.Length; i++)
                    {
                        row.Append(TextCell(Ref(i + 1, (int)rowIndex), values[i]));
                    }

                    rowIndex++;
                }

                wsPart.Worksheet.Save();
                wbPart.Workbook.Save();
            }

            bytes = ms.ToArray();
        }

        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Memos.xlsx";
        FileUtil.SaveAs(JSRuntimeInjector, fileName, bytes);
    }

    // OpenXML helpers
    private static Cell TextCell(string cellRef, string text) =>
        new Cell
        {
            CellReference = cellRef,
            DataType = CellValues.String,
            CellValue = new CellValue(text ?? string.Empty)
        };

    private static string Ref(int col1Based, int row) => $"{ColName(col1Based)}{row}";

    private static string ColName(int index)
    {
        var dividend = index;
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
        if (sortOrder == "") sortOrder = "Create";
        else if (sortOrder == "Create") sortOrder = "CreateDesc";
        else sortOrder = "";
        await DisplayData();
    }

    protected async void SortByName()
    {
        if (!sortOrder.Contains("Name")) sortOrder = "";
        if (sortOrder == "") sortOrder = "Name";
        else if (sortOrder == "Name") sortOrder = "NameDesc";
        else sortOrder = "";
        await DisplayData();
    }

    protected async void SortByTitle()
    {
        if (!sortOrder.Contains("Title")) sortOrder = "";
        if (sortOrder == "") sortOrder = "Title";
        else if (sortOrder == "Title") sortOrder = "TitleDesc";
        else sortOrder = "";
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
        try
        {
            if (AuthenticationStateProviderRef is null || UserManagerRef is null)
            {
                UserId = "";
                UserName = "Anonymous";
                return;
            }

            var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
            var user = authState?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var currentUser = await UserManagerRef.GetUserAsync(user);
                UserId = currentUser?.Id ?? "";
                UserName = user?.Identity?.Name ?? currentUser?.UserName ?? "Anonymous";
            }
            else
            {
                UserId = "";
                UserName = "Anonymous";
            }
        }
        catch
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }
    #endregion

    private async Task OpenPreview(long attachmentId)
    {
        await JSRuntimeInjector.InvokeVoidAsync("attachmentFunctions.openPreview", attachmentId);
    }
}
