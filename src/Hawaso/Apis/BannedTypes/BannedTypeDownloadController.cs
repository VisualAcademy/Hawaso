using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VisualAcademy.Models.BannedTypes;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Apis.BannedTypes
{
    [Authorize(Roles = "Administrators")]
    public class BannedTypeDownloadController : Controller
    {
        private readonly IBannedTypeRepository _repository;

        public BannedTypeDownloadController(IBannedTypeRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/BannedTypes/ExcelDown)
        /// </summary>
        [HttpGet("/BannedTypes/ExcelDown")]
        public async Task<IActionResult> ExcelDown()
        {
            var results = await _repository.GetAllAsync(0, 100);

            // Records가 BannedTypeModel 컬렉션이므로 동일 타입으로 받기 (CS0019 방지)
            var models = results.Records?.ToList() ?? new List<BannedTypeModel>();

            if (models.Count == 0)
                return NotFound("No banned type records found.");

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
                        Name = "BannedTypes"
                    });

                    // ----- Header (A1~E1) -----
                    uint headerRowIndex = 1;
                    var headerRow = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(headerRow);

                    string[] headers = { "Id", "Name", "CreatedAt", "Active", "CreatedBy" };
                    for (int i = 0; i < headers.Length; i++)
                        headerRow.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));

                    // ----- Data (A2~) -----
                    uint rowIndex = 2;
                    foreach (var m in models)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        // CreatedAt: DateTimeOffset → 문자열 포맷(필요 시 타임존 변환 로직 추가 가능)
                        var createdStr = m.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                        var values = new[]
                        {
                            m.Id.ToString(),
                            m.Name ?? string.Empty,
                            createdStr,
                            (m.Active?.ToString() ?? string.Empty),
                            m.CreatedBy ?? string.Empty
                        };

                        for (int i = 0; i < values.Length; i++)
                            row.Append(TextCell(Ref(i + 1, (int)rowIndex), values[i]));

                        rowIndex++;
                    }

                    wsPart.Worksheet.Save();
                    wbPart.Workbook.Save();
                }

                bytes = ms.ToArray();
            }

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_BannedTypes.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
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
