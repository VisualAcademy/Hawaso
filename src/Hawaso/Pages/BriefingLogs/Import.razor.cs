using BlazorInputFile;
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

        public List<BriefingLog> Models { get; set; } = new();

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
                    var uploadedFileName =
                        await FileStorageManager.UploadAsync(
                            file.Data,
                            file.Name,
                            "",
                            overwrite: true);

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

            if (selectedFiles is not { Length: > 0 })
            {
                return;
            }

            var file = selectedFiles.First();

            // xlsx만 지원
            if (!file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                StateHasChanged();
                return;
            }

            using var stream = new MemoryStream();
            await file.Data.CopyToAsync(stream);
            stream.Position = 0;

            using var doc = SpreadsheetDocument.Open(stream, false);

            var wbPart = doc.WorkbookPart;
            if (wbPart?.Workbook?.Sheets == null)
            {
                StateHasChanged();
                return;
            }

            var sheet = wbPart.Workbook.Sheets
                .Elements<Sheet>()
                .FirstOrDefault();

            if (sheet?.Id == null)
            {
                StateHasChanged();
                return;
            }

            var wsPart = wbPart.GetPartById(sheet.Id!) as WorksheetPart;

            if (wsPart?.Worksheet == null)
            {
                StateHasChanged();
                return;
            }

            var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();

            if (sheetData == null)
            {
                StateHasChanged();
                return;
            }

            Models.Clear();

            // 첫 행은 헤더 → 2행부터
            foreach (var row in sheetData
                .Elements<Row>()
                .Where(r => r.RowIndex != null && r.RowIndex.Value >= 2))
            {
                var nameCell = GetCell(row, "A");
                var downCell = GetCell(row, "B");

                var name =
                    GetCellString(wbPart, nameCell)?
                    .Trim() ?? string.Empty;

                var downStr =
                    GetCellString(wbPart, downCell)?
                    .Trim();

                int.TryParse(downStr, out int down);

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

        // ===== OpenXML helpers =====

        private static Cell? GetCell(Row row, string columnName)
        {
            return row.Elements<Cell>()
                .FirstOrDefault(c =>
                {
                    var r = c.CellReference?.Value;
                    return r != null &&
                        GetColumnName(r)
                        .Equals(columnName,
                        StringComparison.OrdinalIgnoreCase);
                });
        }

        private static string GetColumnName(string cellReference)
        {
            return new string(
                cellReference
                .Where(char.IsLetter)
                .ToArray());
        }

        private static string GetCellString(
            WorkbookPart wbPart,
            Cell? cell)
        {
            if (cell == null)
                return string.Empty;

            // Inline
            if (cell.DataType?.Value == CellValues.InlineString)
            {
                return cell.InlineString?.Text?.Text ?? string.Empty;
            }

            // Shared String
            if (cell.DataType?.Value == CellValues.SharedString)
            {
                if (int.TryParse(cell.CellValue?.Text, out var sstIndex))
                {
                    var sst =
                        wbPart.SharedStringTablePart?
                        .SharedStringTable;

                    var item =
                        sst?
                        .Elements<SharedStringItem>()
                        .ElementAtOrDefault(sstIndex);

                    return item?.InnerText ?? string.Empty;
                }

                return string.Empty;
            }

            return cell.CellValue?.Text ?? string.Empty;
        }
    }
}