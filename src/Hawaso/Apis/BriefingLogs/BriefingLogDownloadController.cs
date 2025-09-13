using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zero.Models;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Apis.BriefingLogs
{
    public class BriefingLogDownloadController : Controller
    {
        private readonly IBriefingLogRepository _repository;
        private readonly IBriefingLogFileStorageManager _fileStorageManager;

        public BriefingLogDownloadController(
            IBriefingLogRepository repository,
            IBriefingLogFileStorageManager fileStorageManager)
        {
            _repository = repository;
            _fileStorageManager = fileStorageManager;
        }

        /// <summary>
        /// 게시판 파일 강제 다운로드 기능(/BriefingLogs/BoardDown/{id})
        /// </summary>
        [HttpGet("/BriefingLogs/BoardDown/{id:int}")]
        public async Task<IActionResult> FileDown(int id)
        {
            var model = await _repository.GetByIdAsync(id);
            if (model == null)
                return NotFound();

            if (string.IsNullOrEmpty(model.FileName))
                return NotFound("No file to download.");

            var fileBytes = await _fileStorageManager.DownloadAsync(model.FileName, "BriefingLogs");
            if (fileBytes == null)
                return NotFound("Stored file not found.");

            // 다운로드 카운트 증가
            model.DownCount = model.DownCount + 1;
            await _repository.EditAsync(model);

            return File(fileBytes, "application/octet-stream", model.FileName);
        }

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/BriefingLogs/ExcelDown)
        /// </summary>
        [HttpGet("/BriefingLogs/ExcelDown")]
        public async Task<IActionResult> ExcelDown()
        {
            var results = await _repository.GetAllAsync(0, 100);

            // null 안전 처리: Records가 null이면 빈 컬렉션
            var models = (results.Records ?? Enumerable.Empty<BriefingLog>()).ToList();
            if (models.Count == 0)
                return NotFound("No briefing log records found.");

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
                        Name = "BriefingLogs"
                    });

                    // ----- Header: A1~E1 (Created, Name, Title, DownCount, FileName)
                    uint headerRowIndex = 1;
                    var headerRow = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(headerRow);

                    string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
                    for (int i = 0; i < headers.Length; i++)
                        headerRow.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));

                    // ----- Data: from A2
                    uint rowIndex = 2;
                    foreach (var m in models)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        // Created: DateTime? → string (원 코드 포맷 유지: "yyyy MMM d ddd")
                        var createdStr = m.Created?.ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture) ?? string.Empty;

                        var values = new[]
                        {
                            createdStr,
                            m.Name ?? string.Empty,
                            m.Title ?? string.Empty,
                            m.DownCount.ToString(CultureInfo.InvariantCulture),
                            m.FileName ?? string.Empty
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

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_BriefingLogs.xlsx";
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
