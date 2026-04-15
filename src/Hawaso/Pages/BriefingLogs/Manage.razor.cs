using BlazorUtils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using Zero.Models;

// Open XML SDK
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Pages.BriefingLogs;

public partial class Manage
{
    [Parameter] public int ParentId { get; set; } = 0;
    [Parameter] public string ParentKey { get; set; } = string.Empty;

    [Inject] public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; } = default!;
    [Inject] public NavigationManager NavigationManagerReference { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] public IBriefingLogFileStorageManager FileStorageManager { get; set; } = default!;

    /// <summary>EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기</summary>
    public Components.EditorForm? EditorFormReference { get; set; }

    /// <summary>DeleteDialog에 대한 참조: 모달로 항목 삭제하기</summary>
    public Components.DeleteDialog? DeleteDialogReference { get; set; }

    protected List<BriefingLog> models = new();
    protected BriefingLog model = new();

    /// <summary>공지사항으로 올리기 폼을 띄울건지 여부</summary>
    public bool IsInlineDialogShow { get; set; } = false;

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    public string EditorFormTitle { get; set; } = "CREATE";

    private string searchQuery = string.Empty;
    private string sortOrder = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await DisplayData();
    }

    private async Task DisplayData()
    {
        if (!string.IsNullOrEmpty(ParentKey))
        {
            var articleSet = await UploadRepositoryAsyncReference.GetArticles<string>(
                pager.PageIndex,
                pager.PageSize,
                string.Empty,
                searchQuery,
                sortOrder,
                ParentKey);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items?.ToList() ?? new List<BriefingLog>();
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
            models = articleSet.Items?.ToList() ?? new List<BriefingLog>();
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
            models = articleSet.Items?.ToList() ?? new List<BriefingLog>();
        }

        await InvokeAsync(StateHasChanged);
    }

    protected void NameClick(int id)
    {
        NavigationManagerReference.NavigateTo($"/BriefingLogs/Details/{id}");
    }

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();
        await InvokeAsync(StateHasChanged);
    }

    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        model = new BriefingLog { ParentKey = ParentKey };
        EditorFormReference?.Show();
    }

    protected void EditBy(BriefingLog selectedModel)
    {
        model = selectedModel ?? new BriefingLog();
        EditorFormTitle = "EDIT";
        model.ParentKey = ParentKey;
        EditorFormReference?.Show();
    }

    protected void DeleteBy(BriefingLog selectedModel)
    {
        model = selectedModel ?? new BriefingLog();
        DeleteDialogReference?.Show();
    }

    protected void ToggleBy(BriefingLog selectedModel)
    {
        model = selectedModel ?? new BriefingLog();
        IsInlineDialogShow = true;
    }

    protected async Task DownloadBy(BriefingLog selectedModel)
    {
        if (selectedModel is null || string.IsNullOrWhiteSpace(selectedModel.FileName))
        {
            return;
        }

        byte[] fileBytes = await FileStorageManager.DownloadAsync(selectedModel.FileName, "");

        if (fileBytes is { Length: > 0 })
        {
            selectedModel.DownCount += 1;
            await UploadRepositoryAsyncReference.EditAsync(selectedModel);
            await FileUtil.SaveAs(JSRuntime, selectedModel.FileName, fileBytes);
        }
    }

    // Manage.razor 쪽에서 void 반환형 콜백을 기대하는 경우가 있어 async void 유지
    protected async void CreateOrEdit()
    {
        EditorFormReference?.Hide();
        model = new BriefingLog();

        await DisplayData();
    }

    protected async void DeleteClick()
    {
        string? fileName = model.FileName;

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            await FileStorageManager.DeleteAsync(fileName, "");
        }

        await UploadRepositoryAsyncReference.DeleteAsync(model.Id);

        DeleteDialogReference?.Hide();
        model = new BriefingLog();

        await DisplayData();
    }

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        model = new BriefingLog();
    }

    protected async void ToggleClick()
    {
        model.IsPinned = !model.IsPinned;

        await UploadRepositoryAsyncReference.EditAsync(model);

        IsInlineDialogShow = false;
        model = new BriefingLog();

        await DisplayData();
    }

    #region Search
    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        searchQuery = query ?? string.Empty;

        await DisplayData();
        await InvokeAsync(StateHasChanged);
    }
    #endregion

    // ===== EPPlus 제거: Open XML SDK로 대체 =====
    protected async Task DownloadExcel()
    {
        if (models.Count == 0)
        {
            return;
        }

        using var ms = new MemoryStream();

        using (var doc = SpreadsheetDocument.Create(
            ms,
            DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook,
            true))
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
                Name = "BriefingLogs"
            });

            uint rowIndex = 1;
            var header = new Row { RowIndex = rowIndex };
            sheetData.Append(header);

            string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
            for (int i = 0; i < headers.Length; i++)
            {
                header.Append(TextCell(Ref(i + 1, (int)rowIndex), headers[i]));
            }

            rowIndex = 2;

            foreach (var item in models)
            {
                var row = new Row { RowIndex = rowIndex };
                sheetData.Append(row);

                string createdStr = item.Created.HasValue
                    ? item.Created.Value.ToLocalTime().ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture)
                    : string.Empty;

                string downStr = item.DownCount.ToString(CultureInfo.InvariantCulture);

                string[] values =
                {
                    createdStr,
                    item.Name ?? string.Empty,
                    item.Title ?? string.Empty,
                    downStr,
                    item.FileName ?? string.Empty
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

        string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_BriefingLogs.xlsx";
        await FileUtil.SaveAs(JSRuntime, fileName, ms.ToArray());
    }

    // ===== OpenXML helpers =====
    private static Cell TextCell(string cellRef, string text) =>
        new()
        {
            CellReference = cellRef,
            DataType = CellValues.String,
            CellValue = new CellValue(text ?? string.Empty)
        };

    private static string Ref(int col1Based, int row) => $"{ColName(col1Based)}{row}";

    private static string ColName(int index)
    {
        int dividend = index;
        string col = string.Empty;

        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            col = (char)('A' + modulo) + col;
            dividend = (dividend - modulo) / 26;
        }

        return col;
    }

    #region Sorting
    protected async void SortByName()
    {
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

    protected async void SortByTitle()
    {
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
}