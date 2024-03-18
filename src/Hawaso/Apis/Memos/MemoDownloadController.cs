using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Hawaso.Controllers;

[Authorize]
public class MemoDownloadController(IMemoRepository repository, IMemoFileStorageManager fileStorageManager) : Controller
{
    private readonly string moduleName = "Memos";

    /// <summary>
    /// 게시판 파일 강제 다운로드 기능(/BoardDown/:Id)
    /// </summary>
    public async Task<IActionResult> FileDown(int id)
    {
        var model = await repository.GetByIdAsync(id);

        if (model == null)
        {
            return NotFound(); // 존재하지 않는 리소스에 대해 404 오류 반환
        }
        else
        {
            if (!string.IsNullOrEmpty(model.FileName))
            {
                byte[] fileBytes = await fileStorageManager.DownloadAsync(model.FileName, moduleName);
                if (fileBytes != null)
                {
                    model.DownCount += 1;
                    await repository.EditAsync(model);

                    return File(fileBytes, "application/octet-stream", model.FileName);
                }
                else
                {
                    // 파일이 Memos 폴더에 없을 경우, placeholder.png 파일을 대신 반환
                    var placeholderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "file-not-found.png");
                    byte[] placeholderBytes = await System.IO.File.ReadAllBytesAsync(placeholderPath);
                    return File(placeholderBytes, "image/png", "file-not-found.png");
                }
            }

            // 파일명이 비어있는 경우, 또는 다른 이유로 파일을 처리하지 못한 경우
            // 사용자에게 적절한 메시지와 함께 오류 페이지나 기본 페이지로 리디렉션할 수 있습니다.
            // 여기서는 간단히 NotFound를 반환합니다.
            return NotFound();
        }
    }

    /// <summary>
    /// 엑셀 파일 강제 다운로드 기능(/ExcelDown)
    /// </summary>
    public async Task<IActionResult> ExcelDown()
    {
        var results = await repository.GetAllAsync(0, 100); // 총 몇개를 포함할건지, 나중에 이 부분 업데이트할 것...

        var models = results.Records.ToList();

        if (models != null)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(moduleName);

                var tableBody = worksheet.Cells["A1:A1"].LoadFromCollection((from m in models select new { m.Id, Created = m.Created?.LocalDateTime.ToString(), m.Name, m.Title, m.DownCount, m.FileName }), true);

                var uploadCol = tableBody.Offset(1, 1, models.Count, 1);

                // 그라데이션 효과 부여 
                var rule = uploadCol.ConditionalFormatting.AddThreeColorScale();
                rule.LowValue.Color = Color.SkyBlue;
                rule.MiddleValue.Color = Color.White;
                rule.HighValue.Color = Color.Red;

                var header = worksheet.Cells["B2:F2"];
                worksheet.DefaultColWidth = 25;
                worksheet.Cells[3, 2, models.Count + 2, 2].Style.Numberformat.Format = "yyyy MMM d DDD";
                tableBody.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                tableBody.Style.Fill.PatternType = ExcelFillStyle.Solid;
                tableBody.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
                tableBody.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                header.Style.Font.Bold = true;
                header.Style.Font.Color.SetColor(Color.White);
                header.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                return File(package.GetAsByteArray(), "application/octet-stream", $"{DateTime.Now.ToString("yyyyMMddhhmmss")}_{moduleName}.xlsx");
            }
        }
        return Redirect("/");
    }
}
