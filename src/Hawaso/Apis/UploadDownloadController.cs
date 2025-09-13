using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

using VisualAcademy.Models.Replys; 

namespace Hawaso.Apis
{
    public class UploadDownloadController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IUploadRepository _repository;
        private readonly IFileStorageManager _fileStorageManager;

        public UploadDownloadController(
            IWebHostEnvironment environment,
            IUploadRepository repository,
            IFileStorageManager fileStorageManager)
        {
            _environment = environment;
            _repository = repository;
            _fileStorageManager = fileStorageManager;
        }

        /// <summary>
        /// 게시판 파일 강제 다운로드 기능(/UploadDownload/FileDown/:id)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FileDown(int id)
        {
            var model = await _repository.GetByIdAsync(id);
            if (model is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.FileName))
            {
                return NotFound("No file attached.");
            }

            // 원본 코드와 동일하게 컨테이너는 빈 문자열 사용
            var fileBytes = await _fileStorageManager.DownloadAsync(model.FileName, "");
            if (fileBytes is not null && fileBytes.Length > 0)
            {
                model.DownCount = (model.DownCount) + 1; // Upload.DownCount는 int (non-nullable)
                await _repository.EditAsync(model);

                var encoded = WebUtility.UrlEncode(model.FileName);
                return File(fileBytes, "application/octet-stream", encoded);
            }

            // 저장소에서 파일을 찾지 못한 경우 placeholder 제공(있다면)
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var placeholderPath = Path.Combine(webRoot, "images", "file-not-found.png");
            if (System.IO.File.Exists(placeholderPath))
            {
                var placeholderBytes = await System.IO.File.ReadAllBytesAsync(placeholderPath);
                return File(placeholderBytes, "image/png", "file-not-found.png");
            }

            return NotFound("File not found in storage.");
        }

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/UploadDownload/ExcelDown) - Open XML SDK
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExcelDown()
        {
            var results = await _repository.GetAllAsync(0, 100);
            var models = (results.Records ?? Enumerable.Empty<Upload>()).ToList();
            if (models.Count == 0)
            {
                return NotFound("No records to export.");
            }

            // 투영: 필요한 컬럼만
            var rows = models.Select(m => new
            {
                m.Created,
                m.Name,
                m.Title,
                m.DownCount,
                m.FileName
            }).ToList();

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
                {
                    var wbPart = doc.AddWorkbookPart();
                    wbPart.Workbook = new Workbook();

                    // 스타일 파트(옵션): 기본 텍스트 스타일만 사용
                    var styles = wbPart.AddNewPart<WorkbookStylesPart>();
                    styles.Stylesheet = new Stylesheet();
                    styles.Stylesheet.Save();

                    var wsPart = wbPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    wsPart.Worksheet = new Worksheet(
                        new Columns(
                            // 대략적 열 너비: Created, Name, Title, DownCount, FileName
                            new Column { Min = 1, Max = 1, Width = 22, CustomWidth = true },
                            new Column { Min = 2, Max = 2, Width = 18, CustomWidth = true },
                            new Column { Min = 3, Max = 3, Width = 40, CustomWidth = true },
                            new Column { Min = 4, Max = 4, Width = 12, CustomWidth = true },
                            new Column { Min = 5, Max = 5, Width = 40, CustomWidth = true }
                        ),
                        sheetData
                    );

                    var sheets = wbPart.Workbook.AppendChild(new Sheets());
                    sheets.Append(new Sheet
                    {
                        Id = wbPart.GetIdOfPart(wsPart),
                        SheetId = 1U,
                        Name = "Uploads"
                    });

                    // 헤더
                    uint headerRowIndex = 1;
                    var header = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(header);
                    string[] headers = { "Created", "Name", "Title", "DownCount", "FileName" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        header.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                    }

                    // 데이터
                    uint rowIndex = 2;
                    foreach (var r in rows)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        var values = new[]
                        {
                            ToLocalDateTimeString(r.Created),
                            r.Name ?? string.Empty,
                            r.Title ?? string.Empty,
                            r.DownCount.ToString(CultureInfo.InvariantCulture),
                            r.FileName ?? string.Empty
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

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Uploads.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ===== Helpers =====

        private static string ToLocalDateTimeString(DateTime? dt)
            => dt.HasValue
                ? dt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                : string.Empty;

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
