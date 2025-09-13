using BlazorInputFile;
// Open XML SDK
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs
{
    public partial class Import
    {
        [Inject] public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; } = default!;
        [Inject] public NavigationManager NavigationManagerReference { get; set; } = default!;
        [Inject] public IBriefingLogFileStorageManager FileStorageManager { get; set; } = default!;

        protected BriefingLog model = new BriefingLog();

        public string ParentId { get; set; } = string.Empty;

        protected int[] parentIds = { 1, 2, 3 };

        public List<BriefingLog> Models { get; set; } = new List<BriefingLog>();

        private IFileListEntry[]? selectedFiles;

        protected async Task FormSubmit()
        {
            int.TryParse(ParentId, out int parentId);
            model.ParentId = parentId;

            // 첨부 파일 업로드
            if (selectedFiles is { Length: > 0 })
            {
                var file = selectedFiles.FirstOrDefault();
                if (file != null)
                {
                    var uploadedFileName = await FileStorageManager.UploadAsync(file.Data, file.Name, "", overwrite: true);
                    model.FileName = uploadedFileName;
                    model.FileSize = (int)file.Size;
                }
            }

            // 업로드한 파일 정보 공통 반영 후 저장
            foreach (var m in Models)
            {
                m.FileName = model.FileName;
                m.FileSize = model.FileSize;
                await UploadRepositoryAsyncReference.AddAsync(m);
            }

            NavigationManagerReference.NavigateTo("/BriefingLogs");
        }

        protected async Task HandleSelection(IFileListEntry[] files)
        {
            selectedFiles = files;

            if (selectedFiles is { Length: > 0 })
            {
                var file = selectedFiles.First();

                // xlsx만 지원 (OpenXML)
                if (!file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    // 필요 시 사용자 알림 로직 추가
                    StateHasChanged();
                    return;
                }

                using var stream = new MemoryStream();
                await file.Data.CopyToAsync(stream);
                stream.Position = 0;

                using var doc = SpreadsheetDocument.Open(stream, false);
                var wbPart = doc.WorkbookPart!;
                var sheet = wbPart.Workbook.Sheets!.Elements<Sheet>().FirstOrDefault();
                if (sheet == null)
                {
                    StateHasChanged();
                    return;
                }

                var wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id!);
                var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();
                if (sheetData == null)
                {
                    StateHasChanged();
                    return;
                }

                Models.Clear();

                // 첫 행은 헤더로 가정 -> 2행부터
                foreach (var row in sheetData.Elements<Row>().Where(r => r.RowIndex != null && r.RowIndex.Value >= 2))
                {
                    // A열: Name, B열: DownCount
                    var nameCell = GetCell(row, "A");
                    var downCell = GetCell(row, "B");

                    var name = GetCellString(wbPart, nameCell)?.Trim() ?? string.Empty;
                    var downStr = GetCellString(wbPart, downCell)?.Trim();

                    int down = 0;
                    _ = int.TryParse(downStr, out down);

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        Models.Add(new BriefingLog
                        {
                            Name = name,
                            DownCount = down
                        });
                    }
                }

                StateHasChanged();
            }
        }

        // ===== OpenXML helpers =====

        private static Cell? GetCell(Row row, string columnName)
        {
            // row 내부의 셀 중, 참조가 같은 열(column)을 가리키는 셀 찾기 (A, B, C ...)
            return row.Elements<Cell>()
                      .FirstOrDefault(c =>
                      {
                          var r = c.CellReference?.Value;
                          return r != null && GetColumnName(r).Equals(columnName, StringComparison.OrdinalIgnoreCase);
                      });
        }

        private static string GetColumnName(string cellReference)
        {
            // "B12" -> "B"
            var columnName = new string(cellReference.Where(char.IsLetter).ToArray());
            return columnName;
        }

        private static string GetCellString(WorkbookPart wbPart, Cell? cell)
        {
            if (cell == null) return string.Empty;

            // Inline 문자열
            if (cell.DataType?.Value == CellValues.InlineString)
            {
                return cell.InlineString?.Text?.Text ?? string.Empty;
            }

            // Shared String Table
            if (cell.DataType?.Value == CellValues.SharedString)
            {
                if (int.TryParse(cell.CellValue?.Text, out var sstIndex))
                {
                    var sst = wbPart.SharedStringTablePart?.SharedStringTable;
                    var item = sst?.Elements<SharedStringItem>().ElementAtOrDefault(sstIndex);
                    if (item != null)
                    {
                        // 텍스트가 여러 부분으로 나뉘는 경우도 있으므로 InnerText 사용
                        return item.InnerText ?? string.Empty;
                    }
                }
                return string.Empty;
            }

            // 숫자/일반 텍스트
            return cell.CellValue?.Text ?? string.Empty;
        }
    }
}
