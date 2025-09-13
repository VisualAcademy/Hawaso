using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisualAcademy.Models.Libraries;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Hawaso.Apis.Libraries
{
    public class LibraryDownloadController : Controller
    {
        private const string ModuleName = "Libraries";
        private readonly ILibraryRepository _repository;
        private readonly ILibraryFileStorageManager _fileStorageManager;

        public LibraryDownloadController(ILibraryRepository repository, ILibraryFileStorageManager fileStorageManager)
        {
            _repository = repository;
            _fileStorageManager = fileStorageManager;
        }

        /// <summary>
        /// 게시판 파일 강제 다운로드 기능(/BoardDown/:Id)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FileDown(int id)
        {
            var model = await _repository.GetByIdAsync(id);
            if (model is null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.FileName))
                return NotFound("No file associated with this record.");

            var fileBytes = await _fileStorageManager.DownloadAsync(model.FileName, ModuleName);
            if (fileBytes is null) return NotFound("Stored file not found.");

            // 다운로드수 증가 저장
            model.DownCount = model.DownCount + 1;
            await _repository.EditAsync(model);

            return File(fileBytes, "application/octet-stream", model.FileName);
        }

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/Libraries/ExcelDown)
        /// </summary>
        [HttpGet("/Libraries/ExcelDown")]
        public async Task<IActionResult> ExcelDown()
        {
            var results = await _repository.GetAllAsync(0, 100);

            // results는 값 타입일 수 있으므로 ?. 금지
            var models = (results.Records ?? Enumerable.Empty<LibraryModel>()).ToList();

            if (models.Count == 0)
                return NotFound("No library records found.");

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
                        Name = ModuleName
                    });

                    // 헤더: A1~E1 (Created, Name, Title, DownCount, FileName)
                    uint headerRowIndex = 1;
                    var header = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(header);

                    string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
                    for (int i = 0; i < headers.Length; i++)
                        header.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));

                    // 데이터: A2부터
                    uint rowIndex = 2;
                    foreach (var m in models)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        // DateTime?는 Value.ToString(...)으로 포맷
                        var createdStr = m.Created.HasValue
                            ? m.Created.Value.ToString("yyyy MMM d ddd", CultureInfo.InvariantCulture)
                            : string.Empty;

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

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{ModuleName}.xlsx";
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
