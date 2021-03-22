using Hawaso.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class DepartmentDownloadController : Controller
    {
        private readonly IDepartmentRepository _repository;

        public DepartmentDownloadController(IDepartmentRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/ExcelDown)
        /// </summary>
        public async Task<IActionResult> ExcelDown()
        {
            var results = await _repository.GetAllAsync(0, 100);

            var models = results.Records.ToList();

            if (models != null)
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Departments");

                    var tableBody = worksheet.Cells["B2:B2"].LoadFromCollection(
                        (from m in models select new { m.Id, m.Name, CreatedAt = m.CreatedAt.LocalDateTime.ToString(), m.Active, m.CreatedBy })
                        , true);

                    var uploadCol = tableBody.Offset(1, 1, models.Count, 1);

                    // 그라데이션 효과 부여 
                    var rule = uploadCol.ConditionalFormatting.AddThreeColorScale();
                    rule.LowValue.Color = Color.SkyBlue;
                    rule.MiddleValue.Color = Color.White;
                    rule.HighValue.Color = Color.Red;

                    var header = worksheet.Cells["B2:F2"];
                    worksheet.DefaultColWidth = 25;
                    //worksheet.Cells[3, 2, models.Count + 2, 2].Style.Numberformat.Format = "yyyy MMM d DDD";
                    tableBody.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    tableBody.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    tableBody.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
                    tableBody.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    header.Style.Font.Bold = true;
                    header.Style.Font.Color.SetColor(Color.White);
                    header.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    return File(package.GetAsByteArray(), "application/octet-stream", $"{DateTime.Now.ToString("yyyyMMddhhmmss")}_Departments.xlsx");
                }
            }
            return Redirect("/");
        }
    }
}
