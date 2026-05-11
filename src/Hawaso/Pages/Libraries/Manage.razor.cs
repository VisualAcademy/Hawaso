using BlazorUtils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using VisualAcademy.Models.Libraries;
using VisualAcademy.Models.Replys;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Pages.Libraries;

public partial class Manage
{
    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;

    #region Injectors

    [Inject]
    public ILibraryRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    public IFileStorageManager FileStorageManager { get; set; } = default!;

    #endregion

    /// <summary>
    /// EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기
    /// </summary>
    public Components.EditorForm EditorFormReference { get; set; } = default!;

    /// <summary>
    /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기
    /// </summary>
    public Components.DeleteDialog DeleteDialogReference { get; set; } = default!;

    protected List<LibraryModel>? models;

    protected LibraryModel model = new();

    /// <summary>
    /// 공지사항으로 올리기 폼을 띄울건지 여부
    /// </summary>
    public bool IsInlineDialogShow { get; set; } = false;

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    public string EditorFormTitle { get; set; } = "CREATE";

    #region Search / Sorting

    private string searchQuery = string.Empty;

    protected string sortOrder = string.Empty;

    #endregion

    protected override async Task OnInitializedAsync()
    {
        await DisplayData();
    }

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

    protected void NameClick(int id)
    {
        NavigationManagerReference.NavigateTo($"/Libraries/Details/{id}");
    }

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();

        StateHasChanged();
    }

    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";

        model = new LibraryModel
        {
            ParentKey = ParentKey
        };

        EditorFormReference.Show();
    }

    protected void EditBy(LibraryModel selectedModel)
    {
        EditorFormTitle = "EDIT";

        model = selectedModel;
        model.ParentKey = ParentKey;

        EditorFormReference.Show();
    }

    protected void DeleteBy(LibraryModel selectedModel)
    {
        model = selectedModel;

        DeleteDialogReference.Show();
    }

    protected void ToggleBy(LibraryModel selectedModel)
    {
        model = selectedModel;

        IsInlineDialogShow = true;
    }

    protected async void DownloadBy(LibraryModel selectedModel)
    {
        if (string.IsNullOrWhiteSpace(selectedModel.FileName))
        {
            return;
        }

        byte[] fileBytes = await FileStorageManager.DownloadAsync(selectedModel.FileName, string.Empty);

        if (fileBytes.Length == 0)
        {
            return;
        }

        selectedModel.DownCount++;
        await UploadRepositoryAsyncReference.EditAsync(selectedModel);

        await FileUtil.SaveAs(JSRuntime, selectedModel.FileName, fileBytes);
    }

    /// <summary>
    /// EditorForm CreateCallback용.
    /// </summary>
    protected async Task CreateOrEdit()
    {
        await CloseEditorAndRefreshAsync();
    }

    /// <summary>
    /// EditorForm EditCallback용.
    /// EventCallback&lt;bool&gt;와의 연결을 위해 bool 매개변수 오버로드를 둔다.
    /// </summary>
    protected async Task CreateOrEdit(bool _)
    {
        await CloseEditorAndRefreshAsync();
    }

    private async Task CloseEditorAndRefreshAsync()
    {
        EditorFormReference.Hide();

        model = new LibraryModel();

        await DisplayData();
    }

    protected async void DeleteClick()
    {
        if (!string.IsNullOrWhiteSpace(model.FileName))
        {
            await FileStorageManager.DeleteAsync(model.FileName, string.Empty);
        }

        await UploadRepositoryAsyncReference.DeleteAsync(model.Id);

        DeleteDialogReference.Hide();

        model = new LibraryModel();

        await DisplayData();
    }

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;

        model = new LibraryModel();
    }

    protected async void ToggleClick()
    {
        model.IsPinned = model.IsPinned == true ? false : true;

        await UploadRepositoryAsyncReference.EditAsync(model);

        IsInlineDialogShow = false;

        model = new LibraryModel();

        await DisplayData();
    }

    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        pager.PageNumber = 1;

        searchQuery = query ?? string.Empty;

        await DisplayData();

        StateHasChanged();
    }

    #region Excel Download

    // EPPlus 제거, Open XML SDK로 교체
    protected async Task DownloadExcel()
    {
        if (models == null || models.Count == 0)
        {
            return;
        }

        using var ms = new MemoryStream();

        using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
        {
            WorkbookPart wbPart = doc.AddWorkbookPart();
            wbPart.Workbook = new Workbook();

            WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();

            SheetData sheetData = new();
            wsPart.Worksheet = new Worksheet(sheetData);

            Sheets sheets = wbPart.Workbook.AppendChild(new Sheets());

            sheets.Append(new Sheet
            {
                Id = wbPart.GetIdOfPart(wsPart),
                SheetId = 1U,
                Name = "Libraries"
            });

            uint headerRowIndex = 1;

            Row headerRow = new()
            {
                RowIndex = headerRowIndex
            };

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

            uint rowIndex = 2;

            foreach (LibraryModel item in models)
            {
                Row row = new()
                {
                    RowIndex = rowIndex
                };

                sheetData.Append(row);

                string createdText = item.Created?.ToLocalTime()
                    .ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture) ?? string.Empty;

                string[] values =
                {
                    createdText,
                    item.Name ?? string.Empty,
                    item.Title ?? string.Empty,
                    item.DownCount.ToString(CultureInfo.InvariantCulture),
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

        byte[] bytes = ms.ToArray();

        await FileUtil.SaveAs(JSRuntime, $"{DateTime.Now:yyyyMMddHHmmss}_Libraries.xlsx", bytes);
    }

    #endregion

    #region Open XML Helpers

    private static Cell TextCell(string cellRef, string text)
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
        int dividend = index;
        string columnName = string.Empty;

        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            columnName = (char)('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    #endregion

    #region Sorting

    protected async void SortByName()
    {
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
        sortOrder = sortOrder switch
        {
            "" => "Title",
            "Title" => "TitleDesc",
            _ => ""
        };

        await DisplayData();
    }

    #endregion
}