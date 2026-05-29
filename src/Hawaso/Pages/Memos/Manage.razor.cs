using BlazorUtils;
using Hawaso.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Globalization;
using VisualAcademy.Pages.Memos.Components;

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

    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntimeInjector { get; set; } = default!;
    [Inject] public IMemoRepository RepositoryReference { get; set; } = default!;
    [Inject] public IMemoFileStorageManager FileStorageManagerReference { get; set; } = default!;

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

    public ModalForm? EditorFormReference { get; set; }
    public DeleteDialog? DeleteDialogReference { get; set; }

    #endregion

    #region Search

    private string searchQuery = "";

    protected async Task Search(string query)
    {
        pager.PageIndex = 0;
        searchQuery = query;
        await DisplayData();
    }

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(UserName))
        {
            await GetUserIdAndUserName();
        }

        await DisplayData();
    }

    #endregion

    #region DisplayData()

    private async Task DisplayData()
    {
        if (!string.IsNullOrEmpty(ParentKey))
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(
                pager.PageIndex,
                pager.PageSize,
                searchField: "",
                searchQuery,
                sortOrder,
                ParentKey);

            pager.RecordCount = articleSet.TotalCount;
            Models = articleSet.Items.ToList();
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
            Models = articleSet.Items.ToList();
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
            Models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }

    #endregion

    #region Navigation

    protected void NameClick(long id)
    {
        Nav.NavigateTo($"/Memos/Details/{id}");
    }

    #endregion

    protected async Task PageIndexChanged(int pageIndex)
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

        EditorFormReference?.Show();
    }

    protected void EditBy(Memo model)
    {
        EditorFormTitle = "EDIT";

        Model = model;
        Model.ParentId = model.ParentId;
        Model.ParentKey = model.ParentKey;

        EditorFormReference?.Show();
    }

    protected void DeleteBy(Memo model)
    {
        Model = model;
        DeleteDialogReference?.Show();
    }

    protected async Task DownloadBy(Memo model)
    {
        if (string.IsNullOrEmpty(model.FileName))
        {
            return;
        }

        byte[]? fileBytes = await FileStorageManagerReference.DownloadAsync(model.FileName, "Memos");

        if (fileBytes is null)
        {
            return;
        }

        model.DownCount = (model.DownCount ?? 0) + 1;

        await RepositoryReference.EditAsync(model);
        await FileUtil.SaveAs(JSRuntimeInjector, model.FileName, fileBytes);
    }

    protected async Task CreateOrEditOld()
    {
        EditorFormReference?.Hide();

        Model = new Memo();

        await DisplayData();
    }

    protected void CreateOrEdit()
    {
        EditorFormReference?.Hide();
        Nav.NavigateTo(Nav.Uri, forceLoad: true);
    }

    protected async Task DeleteClick()
    {
        if (!string.IsNullOrEmpty(Model.FileName))
        {
            await FileStorageManagerReference.DeleteAsync(Model.FileName, "Memos");
        }

        if (Model.Id > 0)
        {
            await RepositoryReference.DeleteAsync(Model.Id);
        }

        DeleteDialogReference?.Hide();

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

    protected async Task ToggleClick()
    {
        if (Model.Id <= 0)
        {
            IsInlineDialogShow = false;
            Model = new Memo();
            return;
        }

        Model.IsPinned = !Model.IsPinned;

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

    #region Excel

    protected async Task DownloadExcelWithWebApi()
    {
        await FileUtil.SaveAsExcel(JSRuntimeInjector, "/MemoDownload/ExcelDown");
        Nav.NavigateTo("/Memos");
    }

    protected async Task DownloadExcel()
    {
        var rows = Models;

        if (rows.Count == 0)
        {
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

                uint headerRowIndex = 1;
                var header = new Row { RowIndex = headerRowIndex };
                sheetData.Append(header);

                string[] headers =
                [
                    "Created",
                    "Name",
                    "Title",
                    "DownCount",
                    "FileName"
                ];

                for (int i = 0; i < headers.Length; i++)
                {
                    header.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                }

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

        await FileUtil.SaveAs(JSRuntimeInjector, fileName, bytes);
    }

    private static Cell TextCell(string cellRef, string? text)
    {
        return new Cell
        {
            CellReference = cellRef,
            DataType = CellValues.String,
            CellValue = new CellValue(text ?? string.Empty)
        };
    }

    private static string Ref(int col1Based, int row)
    {
        return $"{ColName(col1Based)}{row}";
    }

    private static string ColName(int index)
    {
        var dividend = index;
        var col = string.Empty;

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

    protected async Task SortByCreate()
    {
        if (!sortOrder.Contains("Create"))
        {
            sortOrder = "";
        }

        if (sortOrder == "")
        {
            sortOrder = "Create";
        }
        else if (sortOrder == "Create")
        {
            sortOrder = "CreateDesc";
        }
        else
        {
            sortOrder = "";
        }

        await DisplayData();
    }

    protected async Task SortByName()
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

    protected async Task SortByTitle()
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

    #region Get UserId and UserName

    [Parameter] public string UserId { get; set; } = "";
    [Parameter] public string UserName { get; set; } = "";

    [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; } = default!;
    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = default!;

    private async Task GetUserIdAndUserName()
    {
        try
        {
            var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var currentUser = await UserManagerRef.GetUserAsync(user);

                UserId = currentUser?.Id ?? "";
                UserName = user.Identity.Name ?? currentUser?.UserName ?? "Anonymous";
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