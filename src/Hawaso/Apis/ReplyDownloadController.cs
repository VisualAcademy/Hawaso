using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using VisualAcademy.Models.Replys;

namespace Hawaso.Apis
{
    public class ReplyDownloadController(
        IReplyRepository repository,
        IFileStorageManager fileStorageManager) : Controller
    {
        private const string ModuleName = "Replys";

        /// <summary>
        /// 게시판 파일 강제 다운로드 기능(/ReplyDownload/FileDown/:id)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FileDown(int id)
        {
            var model = await repository.GetByIdAsync(id);
            if (model is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.FileName))
            {
                return NotFound();
            }

            // 원 코드가 "" 컨테이너를 사용하므로 동일하게 유지
            var fileBytes = await fileStorageManager.DownloadAsync(model.FileName, "");
            if (fileBytes is { Length: > 0 })
            {
                model.DownCount = (model.DownCount ?? 0) + 1;
                await repository.EditAsync(model);

                var encoded = WebUtility.UrlEncode(model.FileName);
                return File(fileBytes, "application/octet-stream", encoded);
            }

            // 저장소에 없으면 placeholder 제공(있을 때)
            var placeholderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "file-not-found.png");
            if (System.IO.File.Exists(placeholderPath))
            {
                var placeholderBytes = await System.IO.File.ReadAllBytesAsync(placeholderPath);
                return File(placeholderBytes, "image/png", "file-not-found.png");
            }

            return NotFound();
        }

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/ReplyDownload/ExcelDown) - Open XML SDK 버전
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExcelDown()
        {
            var results = await repository.GetAllAsync(0, 100);

            var models = (results.Records ?? Enumerable.Empty<Reply>()).ToList();
            if (models.Count == 0)
            {
                return NotFound();
            }

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

                    // ===== 헤더 =====
                    // Created, Name, Title, DownCount, FileName
                    uint headerRowIndex = 1;
                    var header = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(header);

                    string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        header.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                    }

                    // ===== 데이터 =====
                    uint rowIndex = 2;
                    foreach (var m in models)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        var values = new[]
                        {
                            ToLocalDateTimeString(m.Created),
                            m.Name ?? string.Empty,
                            m.Title ?? string.Empty,
                            (m.DownCount ?? 0).ToString(CultureInfo.InvariantCulture),
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

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{ModuleName}.xlsx";
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ===== Helpers =====

        private static string ToLocalDateTimeString(object? value)
        {
            // Created가 DateTimeOffset? 또는 DateTime? 어느 쪽이든 대응
            if (value is DateTimeOffset dto)
            {
                return dto.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            if (value is DateTime dt)
            {
                return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            return value?.ToString() ?? string.Empty;
        }

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
            // 1->A, 2->B, ... 26->Z, 27->AA ...
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
