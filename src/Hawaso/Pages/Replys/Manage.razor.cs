using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorUtils;
// EPPlus 제거
// using OfficeOpenXml;
// using System.Drawing;
// using OfficeOpenXml.Style;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using VisualAcademy.Models.Replys;
using Hawaso.Pages.Replys.Components;

// Open XML SDK 추가
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
    public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    [Inject] public NavigationManager Nav { get; set; }
    [Inject] public IJSRuntime JSRuntimeInjector { get; set; }
    [Inject] public IReplyRepository RepositoryReference { get; set; }
    [Inject] public IFileStorageManager FileStorageManagerReference { get; set; }
    #endregion

    #region Properties
    /// <summary>글쓰기 또는 수정하기 폼의 제목</summary>
    public string EditorFormTitle { get; set; } = "CREATE";
    #endregion

    /// <summary>모달로 글쓰기 또는 수정하기</summary>
    public Components.EditorForm EditorFormReference { get; set; }

    /// <summary>모달로 항목 삭제</summary>
    public DeleteDialog DeleteDialogReference { get; set; }

    protected List<Reply> models;

    protected Reply model = new Reply();

    protected DulPager.DulPagerBase pager = new DulPager.DulPagerBase()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync() => await DisplayData();
    #endregion

    private async Task DisplayData()
    {
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
            // 평상시에는 이 코드만 사용
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, 0);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged(); // Refresh
    }

    protected void NameClick(int id) => Nav.NavigateTo($"/Replys/Details/{id}");

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
        this.model = new Reply();
        this.model.ParentKey = ParentKey;
        EditorFormReference.Show();
    }

    protected void EditBy(Reply model)
    {
        EditorFormTitle = "EDIT";
        this.model = new Reply();
        this.model = model;
        this.model.ParentKey = ParentKey;
        EditorFormReference.Show();
    }

    protected void DeleteBy(Reply model)
    {
        this.model = model;
        DeleteDialogReference.Show();
    }
    #endregion

    protected async void DownloadBy(Reply model)
    {
        if (!string.IsNullOrEmpty(model.FileName))
        {
            byte[] fileBytes = await FileStorageManagerReference.DownloadAsync(model.FileName, "");
            if (fileBytes != null)
            {
                // DownCount
                model.DownCount = model.DownCount + 1;
                await RepositoryReference.EditAsync(model);

                await FileUtil.SaveAs(JSRuntimeInjector, model.FileName, fileBytes);
            }
        }
    }

    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();
        this.model = new Reply();
        await DisplayData();
    }

    protected async void DeleteClick()
    {
        if (!string.IsNullOrEmpty(model?.FileName))
        {
            // 첨부 파일 삭제 
            await FileStorageManagerReference.DeleteAsync(model.FileName, "");
        }

        await RepositoryReference.DeleteAsync(this.model.Id);
        DeleteDialogReference.Hide();
        this.model = new Reply(); // 선택했던 모델 초기화
        await DisplayData(); // 다시 로드
    }

    #region Toggle with Inline Dialog
    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        this.model = new Reply();
    }

    protected async void ToggleClick()
    {
        this.model.IsPinned = (this.model?.IsPinned == true) ? false : true;

        // 변경된 내용 업데이트
        await RepositoryReference.EditAsync(this.model);

        IsInlineDialogShow = false; // 표시 속성 초기화
        this.model = new Reply(); // 선택한 모델 초기화 

        await DisplayData(); // 다시 로드
    }

    protected void ToggleBy(Reply model)
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
        FileUtil.SaveAsExcel(JSRuntimeInjector, "/ReplyDownload/ExcelDown");
        Nav.NavigateTo($"/Replys"); // 다운로드 후 현재 페이지 다시 로드
    }

    // EPPlus -> Open XML SDK 로 교체
    protected void DownloadExcel()
    {
        var list = models ?? new List<Reply>();
        if (list.Count == 0) return;

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

                // 헤더
                uint headerRowIndex = 1;
                var headerRow = new Row { RowIndex = headerRowIndex };
                sheetData.Append(headerRow);

                string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                }

                // 데이터
                uint rowIndex = 2;
                foreach (var m in list)
                {
                    var row = new Row { RowIndex = rowIndex };
                    sheetData.Append(row);

                    // Created 안전 변환 (DateTime 또는 DateTimeOffset 모두 대응)
                    string createdStr = string.Empty;
                    // DateTime? 형태라면
                    if (m.Created is DateTime dt)
                    {
                        createdStr = dt.ToLocalTime().ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture);
                    }
                    createdStr = m.Created?.ToString() ?? string.Empty;

                    // DownCount는 int 또는 int? 둘 다 대응
                    string downCountStr = (m.DownCount is int dc ? dc : 0).ToString(CultureInfo.InvariantCulture);

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

        var fileName = $"{System.DateTime.Now:yyyyMMddHHmmss}_Replys.xlsx";
        FileUtil.SaveAs(JSRuntimeInjector, fileName, bytes);
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
    #endregion

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
