using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Azunt.FileManagement;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;

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
            _repository = repository;
            _fileStorage = fileStorage;
        }

        /// <summary>
        /// 파일업로드 리스트 엑셀 다운로드
        /// GET /api/FileDownload/ExcelDown
        /// </summary>
        [HttpGet("ExcelDown")]
        public async Task<IActionResult> ExcelDown()
        {
            var items = await _repository.GetAllAsync();

            if (!items.Any())
            {
                return NotFound("No file records found.");
            }

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Files");

            var range = sheet.Cells["B2"].LoadFromCollection(
                items.Select(m => new
                {
                    m.Id,
                    m.Name,
                    Created = m.Created.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    m.Active,
                    m.CreatedBy
                }),
                PrintHeaders: true
            );

            var header = sheet.Cells["B2:F2"];
            sheet.DefaultColWidth = 22;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
            range.Style.Border.BorderAround(ExcelBorderStyle.Medium);

            header.Style.Font.Bold = true;
            header.Style.Font.Color.SetColor(Color.White);
            header.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

            var activeCol = range.Offset(1, 3, items.Count(), 1);
            var rule = activeCol.ConditionalFormatting.AddThreeColorScale();
            rule.LowValue.Color = Color.Red;
            rule.MiddleValue.Color = Color.White;
            rule.HighValue.Color = Color.Green;

            var content = package.GetAsByteArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{DateTime.Now:yyyyMMddHHmmss}_Files.xlsx");
        }

        /// <summary>
        /// 파일 단일 다운로드
        /// GET /api/FileDownload/{fileName}
        /// </summary>
        [HttpGet("{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                var stream = await _fileStorage.DownloadAsync(fileName);
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
    }
}
