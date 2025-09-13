using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Azunt.DivisionManagement;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Azunt.Apis.Divisions
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Administrators")]
    public class DivisionExportController : ControllerBase
    {
        private readonly IDivisionRepository _repository;

        public DivisionExportController(IDivisionRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Division 목록 엑셀 다운로드
        /// GET /api/DivisionExport/Excel
        /// </summary>
        [HttpGet("Excel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var models = await _repository.GetAllAsync();
            if (models == null || models.Count == 0)
                return NotFound("No division records found.");

            using var ms = new MemoryStream();
            using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                // Workbook / Worksheet
                var wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();

                var wsPart = wbPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                wsPart.Worksheet = new Worksheet(sheetData);

                var sheets = wbPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = wbPart.GetIdOfPart(wsPart),
                    SheetId = 1,
                    Name = "Divisions"
                });

                // ----- 헤더: A1~E1 -----
                uint headerRowIndex = 1;
                var headerRow = new Row { RowIndex = headerRowIndex };
                sheetData.Append(headerRow);

                string[] headers = { "Id", "Name", "CreatedAt", "Active", "CreatedBy" };
                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.Append(CreateTextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                }

                // ----- 데이터: A2부터 -----
                uint rowIndex = 2;
                foreach (var m in models)
                {
                    var row = new Row { RowIndex = rowIndex };
                    sheetData.Append(row);

                    var values = new[]
                    {
                        m.Id.ToString(),
                        m.Name ?? string.Empty,
                        m.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                        (m.Active == true ? "true" : "false"),
                        m.CreatedBy ?? string.Empty
                    };

                    for (int i = 0; i < values.Length; i++)
                    {
                        row.Append(CreateTextCell(Ref(i + 1, (int)rowIndex), values[i]));
                    }

                    rowIndex++;
                }

                wbPart.Workbook.Save();
                wsPart.Worksheet.Save();
            }

            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{DateTime.Now:yyyyMMddHHmmss}_Divisions.xlsx");
        }

        // ===== Helpers (최소 구현) =====
        private static Cell CreateTextCell(string cellRef, string text)
        {
            return new Cell
            {
                CellReference = cellRef,
                DataType = CellValues.String,
                CellValue = new CellValue(text ?? string.Empty)
            };
        }

        private static string Ref(int col1Based, int row) => $"{ColName(col1Based)}{row}";

        // 1 -> A, 2 -> B, ...
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
    }
}
