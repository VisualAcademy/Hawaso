// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using VisualAcademy.Models.Departments;

namespace Hawaso.Apis.Departments
{
    [Authorize(Roles = "Administrators")]
    public class DepartmentDownloadController : Controller
    {
        private readonly IDepartmentRepository _repository;

        public DepartmentDownloadController(IDepartmentRepository repository) => _repository = repository;

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/Departments/ExcelDown)
        /// </summary>
        [HttpGet("/Departments/ExcelDown")]
        public async Task<IActionResult> ExcelDown()
        {
            var results = await _repository.GetAllAsync(0, 100);

            var models = (results.Records ?? Enumerable.Empty<DepartmentModel>()).ToList();
            if (models.Count == 0)
                return NotFound("No department records found.");

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
                        Name = "Departments"
                    });

                    // Header (A1~E1)
                    uint headerRowIndex = 1;
                    var headerRow = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(headerRow);

                    string[] headers = { "Id", "Name", "CreatedAt", "Active", "CreatedBy" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        headerRow.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                    }

                    // Data (from A2)
                    uint rowIndex = 2;
                    foreach (var m in models)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        var values = new[]
                        {
                            m.Id.ToString(CultureInfo.InvariantCulture),
                            m.Name ?? string.Empty,
                            m.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            (m.Active ?? false).ToString(),
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

                bytes = ms.ToArray();
            }

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Departments.xlsx";
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
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
            var dividend = index; // 1 -> A
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
