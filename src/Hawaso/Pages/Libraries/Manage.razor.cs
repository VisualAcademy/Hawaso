using BlazorUtils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using VisualAcademy.Models.Libraries;
using VisualAcademy.Models.Replys;

// Open XML SDK (EPPlus 제거)
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Pages.Libraries;

public partial class Manage
{
    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = "";

    #region Injectors
    [Inject]
    public ILibraryRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;
    #endregion

    /// <summary>
    /// EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기
    /// </summary>
    public Components.EditorForm EditorFormReference { get; set; }

    /// <summary>
    /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기 
    /// </summary>
    public Components.DeleteDialog DeleteDialogReference { get; set; }

    protected List<LibraryModel> models = new();

    protected LibraryModel model = new();

    /// <summary>
    /// 공지사항으로 올리기 폼을 띄울건지 여부 
    /// </summary>
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

    protected void NameClick(int id) => NavigationManagerReference.NavigateTo($"/Libraries/Details/{id}");

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
        this.model = new LibraryModel { ParentKey = ParentKey };
        EditorFormReference.Show();
    }

    protected void EditBy(LibraryModel model)
    {
        EditorFormTitle = "EDIT";
        this.model = model;
        this.model.ParentKey = ParentKey;
        EditorFormReference.Show();
    }

    protected void DeleteBy(LibraryModel model)
    {
        this.model = model;
        DeleteDialogReference.Show();
    }

    protected void ToggleBy(LibraryModel model)
    {
        this.model = model;
        IsInlineDialogShow = true;
    }

    protected async void DownloadBy(LibraryModel model)
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

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    public IFileStorageManager FileStorageManager { get; set; } = default!;

    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();
        this.model = new LibraryModel();
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
        this.model = new LibraryModel();
        await DisplayData();
    }

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        this.model = new LibraryModel();
    }

    protected async void ToggleClick()
    {
        this.model.IsPinned = (this.model?.IsPinned == true) ? false : true;

        await UploadRepositoryAsyncReference.EditAsync(this.model);
        IsInlineDialogShow = false;
        this.model = new LibraryModel();
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

    // ===== EPPlus 제거, Open XML SDK로 교체 =====
    protected void DownloadExcel()
    {
        // 빈 목록이면 바로 종료(원하면 빈 파일 생성으로 바꿀 수 있음)
        if (models is null || models.Count == 0)
        {
            return;
        }

        using var ms = new MemoryStream();
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
                Name = "Libraries"
            });

            // Header
            uint headerRowIndex = 1;
            var headerRow = new Row { RowIndex = headerRowIndex };
            sheetData.Append(headerRow);

            string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
            }

            // Data rows
            uint rowIndex = 2;
            foreach (var m in models)
            {
                var row = new Row { RowIndex = rowIndex };
                sheetData.Append(row);

                // Created은 DateTime?일 가능성 → 문자열로 안전 출력
                string createdStr = m.Created?.ToLocalTime()
                    .ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture) ?? string.Empty;

                var values = new[]
                {
                    createdStr,
                    m.Name ?? string.Empty,
                    m.Title ?? string.Empty,
                    m.DownCount.ToString(CultureInfo.InvariantCulture),
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

        var bytes = ms.ToArray();
        FileUtil.SaveAs(JSRuntime, $"{DateTime.Now:yyyyMMddHHmmss}_Libraries.xlsx", bytes);
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
        // 1 -> A, 2 -> B, ... 26 -> Z, 27 -> AA ...
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
