using BlazorInputFile;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Replys;

public partial class Import
{
    #region Fields
    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles;
    #endregion

    #region Injectors
    [Inject] public IReplyRepository RepositoryReference { get; set; }
    [Inject] public NavigationManager Nav { get; set; }
    [Inject] public IFileStorageManager FileStorageManagerReference { get; set; }
    #endregion

    protected Reply Model = new Reply();
    public string ParentId { get; set; }
    protected int[] parentIds = { 1, 2, 3 };

    /// <summary>
    /// 파일 업로드 버튼 클릭 이벤트 처리기
    /// </summary>
    protected async void FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        Model.ParentId = parentId;

        #region 파일 업로드(선택된 첫 파일 저장)
        if (selectedFiles is { Length: > 0 })
        {
            var file = selectedFiles.FirstOrDefault();
            if (file != null)
            {
                var uploadedName = await FileStorageManagerReference.UploadAsync(file.Data, file.Name, "", true);
                Model.FileName = uploadedName;
                Model.FileSize = Convert.ToInt32(file.Size);
            }
        }
        #endregion

        foreach (var m in Models)
        {
            m.ParentId = Model.ParentId;
            m.FileName = Model.FileName;
            m.FileSize = Model.FileSize;
            await RepositoryReference.AddAsync(m);
        }

        Nav.NavigateTo("/Replys");
    }

    public List<Reply> Models { get; set; } = new List<Reply>();

    protected async void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files;

        // 엑셀 데이터 읽어오기 (xlsx, 첫 시트, 헤더 1행, A: Name, B: DownCount)
        if (selectedFiles is { Length: > 0 })
        {
            var file = selectedFiles.FirstOrDefault();
            if (file is null) return;

            using var stream = new MemoryStream();
            await file.Data.CopyToAsync(stream);
            stream.Position = 0;

            Models.Clear();

            using (var doc = SpreadsheetDocument.Open(stream, false))
            {
                var wbPart = doc.WorkbookPart;
                var firstSheet = wbPart?.Workbook?.Sheets?.Elements<Sheet>()?.FirstOrDefault();
                if (firstSheet is null) { StateHasChanged(); return; }

                var wsPart = (WorksheetPart)wbPart.GetPartById(firstSheet.Id!);
                var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();
                if (sheetData is null) { StateHasChanged(); return; }

                foreach (var row in sheetData.Elements<Row>())
                {
                    // 1행은 헤더
                    if (row.RowIndex is null || row.RowIndex.Value < 2) continue;

                    string name = string.Empty;
                    int downCount = 0;

                    foreach (var cell in row.Elements<Cell>())
                    {
                        var colIndex = GetColumnIndex(cell.CellReference?.Value);
                        var raw = GetCellValue(doc, cell).Trim();

                        if (colIndex == 1) // A열 -> Name
                        {
                            name = raw;
                        }
                        else if (colIndex == 2) // B열 -> DownCount
                        {
                            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out downCount))
                                downCount = 0;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        Models.Add(new Reply
                        {
                            Name = name,
                            DownCount = downCount
                        });
                    }
                }
            }

            StateHasChanged();
        }
    }

    // ===== OpenXML helpers =====

    private static string GetCellValue(SpreadsheetDocument doc, Cell? cell)
    {
        if (cell is null) return string.Empty;

        var value = cell.CellValue?.InnerText ?? string.Empty;

        // 데이터 타입이 없으면 값 그대로
        if (cell.DataType is null) return value;

        var dt = cell.DataType.Value;

        if (dt == CellValues.SharedString)
        {
            var sst = doc.WorkbookPart?.SharedStringTablePart?.SharedStringTable;
            if (sst != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idx))
            {
                var item = sst.ElementAtOrDefault(idx);
                return item?.InnerText ?? string.Empty;
            }
            return string.Empty;
        }
        else if (dt == CellValues.InlineString)
        {
            return cell.InlineString?.Text?.Text ?? cell.InnerText ?? string.Empty;
        }
        else if (dt == CellValues.String)
        {
            return cell.InnerText ?? value;
        }
        else if (dt == CellValues.Boolean)
        {
            return value == "1" ? "TRUE" : "FALSE";
        }

        return value;
    }

    private static int GetColumnIndex(string? cellRef)
    {
        // "B12" -> 2, "AA3" -> 27
        if (string.IsNullOrEmpty(cellRef)) return 0;
        int sum = 0;
        foreach (char c in cellRef)
        {
            if (char.IsLetter(c))
            {
                sum = (sum * 26) + (char.ToUpperInvariant(c) - 'A' + 1);
            }
            else break;
        }
        return sum;
    }
}
