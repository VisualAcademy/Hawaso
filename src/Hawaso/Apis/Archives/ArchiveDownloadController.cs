using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisualAcademy.Models.Archives;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Controllers
{
    [Authorize]
    public class ArchiveDownloadController : Controller
    {
        private readonly IArchiveRepository _repository;
        private readonly IArchiveFileStorageManager _fileStorageManager;

        public ArchiveDownloadController(
            IArchiveRepository repository,
            IArchiveFileStorageManager fileStorageManager)
        {
            _repository = repository;
            _fileStorageManager = fileStorageManager;
        }

        /// <summary>
        /// 게시판 파일 강제 다운로드 기능(/BoardDown/:Id)
        /// </summary>
        [HttpGet("/BoardDown/{id:int}")]
        public async Task<IActionResult> FileDown(int id)
        {
            var model = await _repository.GetByIdAsync(id);
            if (model == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.FileName))
                return NotFound("File name is empty.");

            var fileBytes = await _fileStorageManager.DownloadAsync(model.FileName, "Archives");
            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound("Stored file not found.");

            // 다운로드 수 증가
            model.DownCount += 1;
            await _repository.EditAsync(model);

            return File(fileBytes, "application/octet-stream", model.FileName);
        }

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/ExcelDown)
        /// </summary>
        [HttpGet("/Archives/ExcelDown")]
        public async Task<IActionResult> ExcelDown()
        {
            var results = await _repository.GetAllAsync(0, 100);
            var models = results.Records?.ToList() ?? new List<Archive>();

            if (models == null || models.Count == 0)
                return NotFound("No archive records found.");

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
                        Name = "Archives"
                    });

                    // ----- Header (A1~F1) -----
                    uint headerRowIndex = 1;
                    var headerRow = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(headerRow);

                    string[] headers = { "Id", "Created", "Name", "Title", "DownCount", "FileName" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        headerRow.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                    }

                    // ----- Rows (A2~) -----
                    uint rowIndex = 2;
                    foreach (var m in models)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        var createdStr = m.Created?.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;

                        var values = new[]
                        {
                            m.Id.ToString(),
                            createdStr,
                            m.Name ?? string.Empty,
                            m.Title ?? string.Empty,
                            m.DownCount.ToString(),
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

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Archives.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
