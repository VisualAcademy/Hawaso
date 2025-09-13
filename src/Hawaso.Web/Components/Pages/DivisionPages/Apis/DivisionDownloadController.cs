using Hawaso.Web.Components.Pages.DivisionPages.Models;
using Microsoft.AspNetCore.Mvc;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;                   // DateTime
using System.Collections.Generic;

namespace Hawaso.Web.Components.Pages.DivisionPages.Apis
{
    //[Authorize(Roles = "Administrators")]
    public class DivisionDownloadController : Controller
    {
        private readonly IDivisionRepository _repository;

        public DivisionDownloadController(IDivisionRepository repository) => _repository = repository;

        /// <summary>
        /// 엑셀 파일 강제 다운로드 기능(/ExcelDown)
        /// </summary>
        public async Task<IActionResult> ExcelDown()
        {
            var results = await _repository.GetAllAsync(0, 100); // 구조체라 가정
            var records = results.Records ?? Enumerable.Empty<DivisionModel>();
            var models = records.ToList();

            if (models.Count == 0)
                return Redirect("/");

            using var ms = new MemoryStream();
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
                    SheetId = 1,
                    Name = "Divisions"
                });

                // 헤더 (B2:F2)
                uint headerRowIndex = 2;
                var headerRow = new Row() { RowIndex = headerRowIndex };
                sheetData.Append(headerRow);
                string[] headers = { "Id", "Name", "CreatedAt", "Active", "CreatedBy" };
                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.Append(CreateTextCell(GetRef(2 + i, (int)headerRowIndex), headers[i]));
                }

                // 데이터 (B3부터)
                uint rowIndex = 3;
                foreach (var m in models)
                {
                    var row = new Row() { RowIndex = rowIndex };
                    sheetData.Append(row);

                    var values = new[]
                    {
                        m.Id.ToString(),
                        m.Name ?? string.Empty,
                        m.CreatedAt.LocalDateTime.ToString(), // 필요 시 날짜 서식화 가능
                        m.Active == true ? "true" : "false",
                        m.CreatedBy ?? string.Empty
                    };

                    for (int i = 0; i < values.Length; i++)
                    {
                        row.Append(CreateTextCell(GetRef(2 + i, (int)rowIndex), values[i]));
                    }
                    rowIndex++;
                }

                wbPart.Workbook.Save();
                wsPart.Worksheet.Save();
            }

            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{DateTime.Now:yyyyMMddHHmmss}_Divisions.xlsx");
        }

        // ----- helpers (최소 구현) -----

        private static Cell CreateTextCell(string cellRef, string text)
        {
            return new Cell
            {
                CellReference = cellRef,
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(text ?? string.Empty))
            };
        }

        private static string GetRef(int colIndex, int rowIndex) => $"{ColName(colIndex)}{rowIndex}";

        private static string ColName(int index)
        {
            var dividend = index; // 1=A
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
