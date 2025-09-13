using Azunt.NoteManagement;
// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace Azunt.Apis.Notes
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteExportController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;

        public NoteExportController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        }

        /// <summary>
        /// 게시글 목록 엑셀 다운로드
        /// GET /api/NoteExport/Excel
        /// </summary>
        [HttpGet("Excel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var items = (await _noteRepository.GetAllAsync()).ToList();
            if (items.Count == 0)
                return NotFound("No note records found.");

            byte[] content;
            using (var ms = new MemoryStream())
            {
                using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
                {
                    // workbook / worksheet
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
                        Name = "Notes"
                    });

                    // Header (A1~F1): Id, Name, Title, Category, Created, CreatedBy
                    var headers = new[] { "Id", "Name", "Title", "Category", "Created", "CreatedBy" };
                    var headerRow = new Row { RowIndex = 1U };
                    sheetData.Append(headerRow);
                    for (int i = 0; i < headers.Length; i++)
                    {
                        headerRow.Append(TextCell(Ref(i + 1, 1), headers[i]));
                    }

                    // Data (starting at row 2)
                    uint rowIndex = 2;
                    foreach (var m in items)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        // Created을 문자열로 표기 (간단/호환성 우선)
                        var createdStr = m.Created.ToLocalTime()
                            .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                        var values = new[]
                        {
                            m.Id.ToString(CultureInfo.InvariantCulture),
                            m.Name ?? string.Empty,
                            m.Title ?? string.Empty,
                            m.Category ?? string.Empty,
                            createdStr,
                            m.CreatedBy ?? string.Empty
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

                content = ms.ToArray();
            }

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{DateTime.Now:yyyyMMddHHmmss}_Notes.xlsx");
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
            // 1->A, 26->Z, 27->AA ...
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
    }
}
