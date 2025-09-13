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
    [Parameter] public string ParentKey { get; set; } = "";

    [Inject] public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; } = default!;
    [Inject] public NavigationManager NavigationManagerReference { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] public IBriefingLogFileStorageManager FileStorageManager { get; set; } = default!;

    /// <summary>EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기</summary>
    public Components.EditorForm EditorFormReference { get; set; } = default!;

    /// <summary>DeleteDialog에 대한 참조: 모달로 항목 삭제하기</summary>
    public Components.DeleteDialog DeleteDialogReference { get; set; } = default!;

    protected List<BriefingLog> models = new();
    protected BriefingLog model = new();

    /// <summary>공지사항으로 올리기 폼을 띄울건지 여부</summary>
    public bool IsInlineDialogShow { get; set; } = false;

    protected DulPager.DulPagerBase pager = new DulPager.DulPagerBase()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    protected override async Task OnInitializedAsync() => await DisplayData();

    private async Task DisplayData()
    {
        if (!string.IsNullOrEmpty(ParentKey))
        {
            var articleSet = await UploadRepositoryAsyncReference.GetArticles<string>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, ParentKey);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await UploadRepositoryAsyncReference.GetArticles<int>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, ParentId);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await UploadRepositoryAsyncReference.GetArticles<int>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, 0);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }

    protected void NameClick(int id) => NavigationManagerReference.NavigateTo($"/BriefingLogs/Details/{id}");

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
        this.model = new BriefingLog { ParentKey = ParentKey };
        EditorFormReference.Show();
    }

    protected void EditBy(BriefingLog model)
    {
        EditorFormTitle = "EDIT";
        this.model = model;
        this.model.ParentKey = ParentKey;
        EditorFormReference.Show();
    }

    protected void DeleteBy(BriefingLog model)
    {
        this.model = model;
        DeleteDialogReference.Show();
    }

    protected void ToggleBy(BriefingLog model)
    {
        this.model = model;
        IsInlineDialogShow = true;
    }

    protected async void DownloadBy(BriefingLog model)
    {
        if (!string.IsNullOrEmpty(model.FileName))
        {
            byte[] fileBytes = await FileStorageManager.DownloadAsync(model.FileName, "");
            if (fileBytes != null)
            {
                model.DownCount = model.DownCount + 1;
                await UploadRepositoryAsyncReference.EditAsync(model);
                await FileUtil.SaveAs(JSRuntime, model.FileName, fileBytes);
            }
        }
    }

    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();
        this.model = new BriefingLog();
        await DisplayData();
    }

    protected async void DeleteClick()
    {
        if (!string.IsNullOrEmpty(model?.FileName))
        {
            await FileStorageManager.DeleteAsync(model.FileName, "");
        }

        await UploadRepositoryAsyncReference.DeleteAsync(this.model.Id);
        DeleteDialogReference.Hide();
        this.model = new BriefingLog();
        await DisplayData();
    }

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        this.model = new BriefingLog();
    }

    protected async void ToggleClick()
    {
        this.model.IsPinned = (this.model?.IsPinned == true) ? false : true;
        await UploadRepositoryAsyncReference.EditAsync(this.model);
        IsInlineDialogShow = false;
        this.model = new BriefingLog();
        await DisplayData();
    }

    #region Search
    private string searchQuery = "";

    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        this.searchQuery = query;
        await DisplayData();
        StateHasChanged();
    }
    #endregion

    // ===== EPPlus 제거: Open XML SDK로 대체 =====
    protected void DownloadExcel()
    {
        // 데이터가 없으면 바로 종료
        if (models == null || models.Count == 0) return;

        using var ms = new MemoryStream();
        using (var doc = SpreadsheetDocument.Create(ms, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook, true))
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

            // Header
            uint rowIndex = 1;
            var header = new Row { RowIndex = rowIndex };
            sheetData.Append(header);

            string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
            for (int i = 0; i < headers.Length; i++)
                header.Append(TextCell(Ref(i + 1, (int)rowIndex), headers[i]));

            // Rows
            rowIndex = 2;
            foreach (var m in models)
            {
                var row = new Row { RowIndex = rowIndex };
                sheetData.Append(row);

                // Created: DateTime / DateTimeOffset / string 등 대응
                string createdStr = string.Empty;
                var createdObj = (object?)m.Created;
                if (createdObj is DateTimeOffset dto)
                    createdStr = dto.LocalDateTime.ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture);
                else if (createdObj is DateTime dt)
                    createdStr = dt.ToLocalTime().ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture);
                else if (createdObj != null)
                    createdStr = createdObj.ToString() ?? string.Empty;

                var downObj = (object?)m.DownCount;
                string downStr = downObj?.ToString() ?? "0";

                var values = new[]
                {
                    createdStr,
                    m.Name ?? string.Empty,
                    m.Title ?? string.Empty,
                    downStr,
                    m.FileName ?? string.Empty
                };

                for (int i = 0; i < values.Length; i++)
                    row.Append(TextCell(Ref(i + 1, (int)rowIndex), values[i]));

                rowIndex++;
            }

            wsPart.Worksheet.Save();
            wbPart.Workbook.Save();
        }

        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_BriefingLogs.xlsx";
        FileUtil.SaveAs(JSRuntime, fileName, ms.ToArray());
    }

    // ===== OpenXML helpers =====
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

    #region Sorting
    private string sortOrder = "";

    protected async void SortByName()
    {
        if (sortOrder == "") sortOrder = "Name";
        else if (sortOrder == "Name") sortOrder = "NameDesc";
        else sortOrder = "";

        await DisplayData();
    }

    protected async void SortByTitle()
    {
        if (sortOrder == "") sortOrder = "Title";
        else if (sortOrder == "Title") sortOrder = "TitleDesc";
        else sortOrder = "";

        await DisplayData();
    }
    #endregion
}
