using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorUtils;
using VisualAcademy.Models.Replys;
using System.Globalization;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Pages.Uploads;

public partial class Manage
{
    [Parameter] public int ParentId { get; set; } = 0;
    [Parameter] public string ParentKey { get; set; } = string.Empty;

    [Inject] public IUploadRepository UploadRepositoryAsyncReference { get; set; } = default!;
    [Inject] public NavigationManager NavigationManagerReference { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] public IFileStorageManager FileStorageManager { get; set; } = default!;

    public Hawaso.Pages.Uploads.Components.EditorForm EditorFormReference { get; set; } = default!;
    public Hawaso.Pages.Uploads.Components.DeleteDialog DeleteDialogReference { get; set; } = default!;

    protected List<Upload> models = new();
    protected Upload model = new();

    public bool IsInlineDialogShow { get; set; } = false;

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    protected override async Task OnInitializedAsync() => await DisplayData();

    private async Task DisplayData()
    {
        if (!string.IsNullOrWhiteSpace(ParentKey))
        {
            var articleSet = await UploadRepositoryAsyncReference.GetArticles<string>(
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
            var articleSet = await UploadRepositoryAsyncReference.GetArticles<int>(
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
            var articleSet = await UploadRepositoryAsyncReference.GetArticles<int>(
                pager.PageIndex,
                pager.PageSize,
                string.Empty,
                searchQuery,
                sortOrder,
                0);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }

    protected void NameClick(int id) =>
        NavigationManagerReference.NavigateTo($"/Uploads/Details/{id}");

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();

        StateHasChanged();
    }

    public string EditorFormTitle { get; set; } = "CREATE";

    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";

        model = new Upload
        {
            ParentKey = ParentKey
        };

        EditorFormReference.Show();
    }

    protected void EditBy(Upload selectedModel)
    {
        EditorFormTitle = "EDIT";

        model = selectedModel;
        model.ParentKey = ParentKey;

        EditorFormReference.Show();
    }

    protected void DeleteBy(Upload selectedModel)
    {
        model = selectedModel;

        DeleteDialogReference.Show();
    }

    protected void ToggleBy(Upload selectedModel)
    {
        model = selectedModel;

        IsInlineDialogShow = true;
    }

    protected async void DownloadBy(Upload selectedModel)
    {
        if (string.IsNullOrWhiteSpace(selectedModel.FileName))
        {
            return;
        }

        byte[] fileBytes = await FileStorageManager.DownloadAsync(
            selectedModel.FileName,
            string.Empty);

        if (fileBytes != null && fileBytes.Length > 0)
        {
            selectedModel.DownCount++;

            await UploadRepositoryAsyncReference.EditAsync(selectedModel);

            await FileUtil.SaveAs(JSRuntime, selectedModel.FileName, fileBytes);
        }
    }

    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();

        model = new Upload();

        await DisplayData();

        StateHasChanged();
    }

    protected async void DeleteClick()
    {
        if (!string.IsNullOrWhiteSpace(model.FileName))
        {
            await FileStorageManager.DeleteAsync(model.FileName, string.Empty);
        }

        await UploadRepositoryAsyncReference.DeleteAsync(model.Id);

        DeleteDialogReference.Hide();

        model = new Upload();

        await DisplayData();

        StateHasChanged();
    }

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;

        model = new Upload();
    }

    protected async void ToggleClick()
    {
        model.IsPinned = !model.IsPinned;

        await UploadRepositoryAsyncReference.EditAsync(model);

        IsInlineDialogShow = false;

        model = new Upload();

        await DisplayData();

        StateHasChanged();
    }

    #region Search

    private string searchQuery = string.Empty;

    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        pager.PageNumber = 1;

        searchQuery = query;

        await DisplayData();

        StateHasChanged();
    }

    #endregion

    // ===== EPPlus 제거: OpenXML로 엑셀 생성 =====
    protected async void DownloadExcel()
    {
        if (models.Count == 0)
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
                    Name = "Uploads"
                });

                // Header
                uint headerRowIndex = 1;
                var headerRow = new Row { RowIndex = headerRowIndex };

                sheetData.Append(headerRow);

                string[] headers =
                {
                    "Created",
                    "Name",
                    "Title",
                    "DownCount",
                    "FileName"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                }

                // Rows
                uint rowIndex = 2;

                foreach (var m in models)
                {
                    var row = new Row { RowIndex = rowIndex };

                    sheetData.Append(row);

                    // Created: DateTime? 또는 DateTimeOffset? 모두 지원
                    var createdStr = m.Created?.ToLocalTime()
                        .ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture) ?? string.Empty;

                    var downStr = Convert.ToString(m.DownCount, CultureInfo.InvariantCulture) ?? "0";

                    var values = new[]
                    {
                        createdStr,
                        m.Name ?? string.Empty,
                        m.Title ?? string.Empty,
                        downStr,
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

        await FileUtil.SaveAs(
            JSRuntime,
            $"{DateTime.Now:yyyyMMddHHmmss}_Uploads.xlsx",
            bytes);
    }

    // ===== OpenXML helpers =====
    private static Cell TextCell(string cellRef, string text) =>
        new()
        {
            CellReference = cellRef,
            DataType = CellValues.String,
            CellValue = new CellValue(text ?? string.Empty)
        };

    private static string Ref(int col1Based, int row) =>
        $"{ColName(col1Based)}{row}";

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

    #region Sorting

    private string sortOrder = string.Empty;

    protected async void SortByName()
    {
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

        StateHasChanged();
    }

    protected async void SortByTitle()
    {
        if (sortOrder == string.Empty)
        {
            sortOrder = "Title";
        }
        else if (sortOrder == "Title")
        {
            sortOrder = "TitleDesc";
        }
        else
        {
            sortOrder = string.Empty;
        }

        await DisplayData();

        StateHasChanged();
    }

    #endregion
}