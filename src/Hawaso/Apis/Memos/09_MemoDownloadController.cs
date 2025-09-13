// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace Hawaso.Controllers
{
    [Authorize]
    public class MemoDownloadController : Controller
    {
        private readonly IMemoRepository repository;
        private readonly IMemoFileStorageManager fileStorageManager;

        private readonly string moduleName = "Memos";

        public MemoDownloadController(
            IMemoRepository repository,
            IMemoFileStorageManager fileStorageManager)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.fileStorageManager = fileStorageManager ?? throw new ArgumentNullException(nameof(fileStorageManager));
        }

        /// <summary>
        /// 게시판 파일 강제 다운로드 기능(/MemoDownload/FileDown/:id)
        /// </summary>
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

            var fileBytes = await fileStorageManager.DownloadAsync(model.FileName, moduleName);
            if (fileBytes is { Length: > 0 })
            {
                // DownCount 증가 (null-safe)
                model.DownCount = (model.DownCount ?? 0) + 1;
                await repository.EditAsync(model);

                return File(fileBytes, "application/octet-stream", model.FileName);
            }

            // 스토리지에 파일이 없을 때 placeholder 반환(있으면)
            var placeholderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "file-not-found.png");
            if (System.IO.File.Exists(placeholderPath))
            {
                var placeholderBytes = await System.IO.File.ReadAllBytesAsync(placeholderPath);
                return File(placeholderBytes, "image/png", "file-not-found.png");
            }

            return NotFound();
        }

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/MemoDownload/ExcelDown) - Open XML SDK 버전
        /// </summary>
        public async Task<IActionResult> ExcelDown()
        {
            var results = await repository.GetAllAsync(0, 100); // 필요 시 개수 조정
            var models = (results.Records ?? Enumerable.Empty<Memo>()).ToList();

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
                        Name = moduleName
                    });

                    // ===== 헤더 =====
                    // Id, Created, Name, Title, DownCount, FileName
                    uint headerRowIndex = 1;
                    var header = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(header);

                    string[] headers = { "Id", "Created", "Name", "Title", "DownCount", "FileName" };
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

                        // Created: DateTime?, DateTimeOffset? 모두 대응 (CS8121 방지: object로 박싱 후 판별)
                        var createdObj = (object?)m.Created;
                        string createdStr = ToLocalDateTimeString(createdObj);

                        var values = new[]
                        {
                            m.Id.ToString(CultureInfo.InvariantCulture),
                            createdStr,
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

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{moduleName}.xlsx";
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ===== Helpers =====

        private static string ToLocalDateTimeString(object? value)
        {
            if (value is DateTimeOffset dto)
            {
                return dto.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            if (value is DateTime dt)
            {
                return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            return string.Empty;
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
