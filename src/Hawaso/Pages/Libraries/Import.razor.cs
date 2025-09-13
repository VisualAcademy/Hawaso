using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using VisualAcademy.Models.Libraries;

// Open XML SDK (EPPlus 제거)
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Pages.Libraries;

public partial class Import
{
    [Inject] public ILibraryRepository UploadRepositoryAsyncReference { get; set; } = default!;
    [Inject] public NavigationManager NavigationManagerReference { get; set; } = default!;
    [Inject] public ILibraryFileStorageManager FileStorageManager { get; set; } = default!;

    protected LibraryModel model = new();

    public string ParentId { get; set; } = string.Empty;

    protected int[] parentIds = { 1, 2, 3 };

    public List<LibraryModel> Models { get; set; } = new();

    private IFileListEntry[]? selectedFiles;

    // 폼 제출
    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        model.ParentId = parentId;

        // 파일 업로드
        if (selectedFiles is { Length: > 0 })
        {
            var file = selectedFiles.First();
            if (file is not null)
            {
                var uploadedName = await FileStorageManager.UploadAsync(file.Data, file.Name, "", overwrite: true);

                model.FileName = uploadedName;
                model.FileSize = Convert.ToInt32(file.Size);
            }
        }

        // 벌크 저장
        foreach (var m in Models)
        {
            m.FileName = model.FileName;
            m.FileSize = model.FileSize;
            await UploadRepositoryAsyncReference.AddAsync(m);
        }

        NavigationManagerReference.NavigateTo("/Libraries");
    }

    // 파일 선택 시 엑셀(XLSX) 읽기 - OpenXML 사용
    protected async Task HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files;

        if (selectedFiles is { Length: > 0 })
        {
            var file = selectedFiles.First();
            using var stream = new MemoryStream();
            await file.Data.CopyToAsync(stream);
            stream.Position = 0;

            Models.Clear();
            foreach (var row in ReadRowsFromFirstWorksheet(stream))
            {
                // A열: Name, B열: DownCount
                if (string.IsNullOrWhiteSpace(row.name)) continue;

                Models.Add(new LibraryModel
                {
                    Name = row.name.Trim(),
                    DownCount = row.downCount
                });
            }

            StateHasChanged();
        }
    }

    // ===== OpenXML helpers =====

    private static IEnumerable<(string name, int downCount)> ReadRowsFromFirstWorksheet(Stream xlsxStream)
    {
        using var doc = SpreadsheetDocument.Open(xlsxStream, false);
        var wbPart = doc.WorkbookPart ?? throw new InvalidOperationException("WorkbookPart is missing.");
        var firstSheet = wbPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault()
                         ?? throw new InvalidOperationException("No worksheet found.");
        var wsPart = (WorksheetPart)wbPart.GetPartById(firstSheet.Id!);
        var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>()
                         ?? throw new InvalidOperationException("SheetData is missing.");

        // 첫 행(헤더) 스킵, 2행부터 데이터
        foreach (var row in sheetData.Elements<Row>().Skip(1))
        {
            var a = GetCellString(wbPart, GetCell(row, "A")); // Name
            var bStr = GetCellString(wbPart, GetCell(row, "B")); // DownCount

            int.TryParse(bStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var b);
            yield return (a, b);
        }
    }

    private static Cell? GetCell(Row row, string columnName)
        => row.Elements<Cell>()
              .FirstOrDefault(c => string.Equals(GetColumnName(c.CellReference?.Value ?? ""), columnName, StringComparison.OrdinalIgnoreCase));

    private static string GetColumnName(string cellRef)
    {
        // 예: "B12" -> "B"
        var chars = cellRef.Where(char.IsLetter).ToArray();
        return new string(chars);
    }

    private static string GetCellString(WorkbookPart wbPart, Cell? cell)
    {
        if (cell is null) return string.Empty;

        // SharedString
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            if (int.TryParse(cell.InnerText, out var sstIndex) &&
                wbPart.SharedStringTablePart?.SharedStringTable is { } sst &&
                sstIndex >= 0 && sstIndex < sst.ChildElements.Count)
            {
                return sst.ElementAt(sstIndex).InnerText ?? string.Empty;
            }
            return string.Empty;
        }

        // InlineString
        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.Text?.Text ?? cell.InnerText ?? string.Empty;
        }

        // Boolean
        if (cell.DataType?.Value == CellValues.Boolean)
        {
            return cell.InnerText == "1" ? "TRUE" : "FALSE";
        }

        // Number / Date(serial) / General
        return cell.CellValue?.InnerText ?? cell.InnerText ?? string.Empty;
    }
}
