using Azunt.FileManagement;
// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace Azunt.Apis.Files
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileDownloadController : ControllerBase
    {
        private readonly IFileRepository _repository;
        private readonly IFileStorageService _fileStorage;

        public FileDownloadController(IFileRepository repository, IFileStorageService fileStorage)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        /// <summary>
        /// 파일업로드 리스트 엑셀 다운로드
        /// GET /api/FileDownload/ExcelDown
        /// </summary>
        [HttpGet("ExcelDown")]
        public async Task<IActionResult> ExcelDown()
        {
            var items = (await _repository.GetAllAsync()).ToList();
            if (items.Count == 0)
            {
                return NotFound("No file records found.");
            }

            byte[] content;
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
                        Name = "Files"
                    });

                    // Header
                    var header = new Row { RowIndex = 1U };
                    sheetData.Append(header);
                    string[] headers = { "Id", "Name", "Created", "Active", "CreatedBy" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        header.Append(TextCell(Ref(i + 1, 1), headers[i]));
                    }

                    // Rows
                    uint rowIndex = 2;
                    foreach (var m in items)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        var created = m.Created.ToLocalTime()
                                              .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        var active = (m.Active ?? false) ? "True" : "False";

                        string[] values =
                        {
                            m.Id.ToString(CultureInfo.InvariantCulture),
                            m.Name ?? string.Empty,
                            created,
                            active,
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
                $"{DateTime.Now:yyyyMMddHHmmss}_Files.xlsx"
            );
        }

        /// <summary>
        /// 파일 단일 다운로드
        /// GET /api/FileDownload/{fileName}
        /// </summary>
        [HttpGet("{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("fileName is required.");

            try
            {
                var stream = await _fileStorage.DownloadAsync(fileName);
                if (stream.CanSeek) stream.Position = 0;
                return File(stream, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound($"FileEntity not found: {fileName}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Download error: {ex.Message}");
            }
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
            // 1 -> A, 26 -> Z, 27 -> AA ...
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
