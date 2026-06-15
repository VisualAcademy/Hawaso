using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorUtils;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using VisualAcademy.Models.Replys;
using Hawaso.Pages.Replys.Components;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Pages.Replys;

public partial class Manage
{
    #region Parameters

    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;

    #endregion

    #region Injectors

    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntimeInjector { get; set; } = default!;
    [Inject] public IReplyRepository RepositoryReference { get; set; } = default!;
    [Inject] public IFileStorageManager FileStorageManagerReference { get; set; } = default!;

    #endregion

    #region Properties

    /// <summary>글쓰기 또는 수정하기 폼의 제목</summary>
    public string EditorFormTitle { get; set; } = "CREATE";

    /// <summary>모달로 글쓰기 또는 수정하기</summary>
    public Components.EditorForm EditorFormReference { get; set; } = default!;

    /// <summary>모달로 항목 삭제</summary>
    public DeleteDialog DeleteDialogReference { get; set; } = default!;

    protected List<Reply> models = new();

    protected Reply model = new();

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync() => await DisplayData();

    #endregion

    private async Task DisplayData()
    {
        if (!string.IsNullOrWhiteSpace(ParentKey))
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(
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
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
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
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
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
        Nav.NavigateTo($"/Replys/Details/{id}");

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
        EditorFormTitle = "CREATE";

        model = new Reply
        {
            ParentKey = ParentKey
        };

        EditorFormReference.Show();
    }

    protected void EditBy(Reply selectedModel)
    {
        EditorFormTitle = "EDIT";

        model = selectedModel;
        model.ParentKey = ParentKey;

        EditorFormReference.Show();
    }

    protected void DeleteBy(Reply selectedModel)
    {
        model = selectedModel;

        DeleteDialogReference.Show();
    }

    #endregion

    protected async void DownloadBy(Reply selectedModel)
    {
        if (string.IsNullOrWhiteSpace(selectedModel.FileName))
        {
            return;
        }

        byte[] fileBytes = await FileStorageManagerReference.DownloadAsync(
            selectedModel.FileName,
            string.Empty);

        if (fileBytes != null && fileBytes.Length > 0)
        {
            selectedModel.DownCount++;

            await RepositoryReference.EditAsync(selectedModel);

            await FileUtil.SaveAs(JSRuntimeInjector, selectedModel.FileName, fileBytes);
        }
    }

    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();

        model = new Reply();

        await DisplayData();

        StateHasChanged();
    }

    protected async void DeleteClick()
    {
        if (!string.IsNullOrWhiteSpace(model.FileName))
        {
            await FileStorageManagerReference.DeleteAsync(model.FileName, string.Empty);
        }

        await RepositoryReference.DeleteAsync(model.Id);

        DeleteDialogReference.Hide();

        model = new Reply();

        await DisplayData();

        StateHasChanged();
    }

    #region Toggle with Inline Dialog

    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;

        model = new Reply();
    }

    protected async void ToggleClick()
    {
        model.IsPinned = !model.IsPinned;

        await RepositoryReference.EditAsync(model);

        IsInlineDialogShow = false;

        model = new Reply();

        await DisplayData();

        StateHasChanged();
    }

    protected void ToggleBy(Reply selectedModel)
    {
        model = selectedModel;

        IsInlineDialogShow = true;
    }

    #endregion

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

    #region Excel

    protected void DownloadExcelWithWebApi()
    {
        FileUtil.SaveAsExcel(JSRuntimeInjector, "/ReplyDownload/ExcelDown");

        Nav.NavigateTo("/Replys");
    }

    // EPPlus -> Open XML SDK 로 교체
    protected void DownloadExcel()
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
                    Name = "Replys"
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

                // Data
                uint rowIndex = 2;

                foreach (var m in models)
                {
                    var row = new Row { RowIndex = rowIndex };

                    sheetData.Append(row);

                    var createdStr = m.Created?.ToLocalTime()
                        .ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture) ?? string.Empty;

                    var downCountStr = Convert.ToString(m.DownCount, CultureInfo.InvariantCulture) ?? "0";

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
                        row.Append(TextCell(Ref(i + 1, (int)rowIndex), values[i]));
                    }

                    rowIndex++;
                }

                wsPart.Worksheet.Save();
                wbPart.Workbook.Save();
            }

            bytes = ms.ToArray();
        }

        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Replys.xlsx";

        FileUtil.SaveAs(JSRuntimeInjector, fileName, bytes);
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

    #endregion

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