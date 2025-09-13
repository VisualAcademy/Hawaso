using BlazorInputFile;
// Open XML SDK
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Uploads;

public partial class Import
{
    #region Injectors
    [Inject] public IUploadRepository UploadRepositoryAsyncReference { get; set; }
    [Inject] public NavigationManager NavigationManagerReference { get; set; }
    [Inject] public IFileStorageManager FileStorageManager { get; set; }
    #endregion

    #region Properties/Fields
    public Upload Model { get; set; } = new();   // 초기화 누락 방지
    public string ParentId { get; set; }

    protected int[] parentIds = { 1, 2, 3 };
    private IFileListEntry[] selectedFiles;

    public List<Upload> Models { get; set; } = new List<Upload>();
    #endregion

    protected async void FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        Model.ParentId = parentId;

        // 파일 업로드(선택된 첫 파일만 업로드)
        if (selectedFiles != null && selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();
            if (file != null)
            {
                var savedName = await FileStorageManager.UploadAsync(file.Data, file.Name, "", overwrite: true);
                Model.FileName = savedName;
                Model.FileSize = Convert.ToInt32(file.Size);
            }
        }

        // 파싱된 행들을 저장
        foreach (var m in Models)
        {
            m.FileName = Model.FileName;
            m.FileSize = Model.FileSize;
            m.ParentId = Model.ParentId;
            await UploadRepositoryAsyncReference.AddAsync(m);
        }

        NavigationManagerReference.NavigateTo("/Uploads");
    }

    protected async void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files;

        if (selectedFiles == null || selectedFiles.Length == 0)
        {
            Models.Clear();
            StateHasChanged();
            return;
        }

        var file = selectedFiles.FirstOrDefault();
        if (file == null) return;

        using var stream = new MemoryStream();
        await file.Data.CopyToAsync(stream);
        stream.Position = 0;

        Models.Clear();

        // Open XML로 엑셀 읽기 (첫 번째 시트, 헤더 1행 가정, 데이터는 2행부터)
        using (var doc = SpreadsheetDocument.Open(stream, false))
        {
            var wbPart = doc.WorkbookPart;
            var firstSheet = wbPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
            if (firstSheet == null) { StateHasChanged(); return; }

            var wsPart = (WorksheetPart)wbPart.GetPartById(firstSheet.Id!);
            var rows = wsPart.Worksheet.Descendants<Row>();

            foreach (var row in rows.Skip(1)) // 2행부터 데이터
            {
                var ri = (int)(row.RowIndex?.Value ?? 0);
                if (ri == 0) continue;

                // A열: Name, B열: DownCount
                var name = ReadCellString(wbPart, wsPart, "A" + ri)?.Trim() ?? string.Empty;
                var downText = ReadCellString(wbPart, wsPart, "B" + ri)?.Trim() ?? "0";

                // DownCount 파싱(정수, 소수로 저장된 경우도 대비)
                int downCount = 0;
                if (!int.TryParse(downText, NumberStyles.Any, CultureInfo.InvariantCulture, out downCount))
                {
                    if (double.TryParse(downText, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        downCount = (int)Math.Round(d);
                }

                // 빈 행 스킵(필요 시 조건 조정)
                if (string.IsNullOrWhiteSpace(name) && downCount == 0)
                    continue;

                Models.Add(new Upload
                {
                    Name = name,
                    DownCount = downCount
                });
            }
        }

        StateHasChanged();
    }

    // ===== OpenXML helpers =====
    private static string? ReadCellString(WorkbookPart wbPart, WorksheetPart wsPart, string cellRef)
    {
        var cell = wsPart.Worksheet.Descendants<Cell>().FirstOrDefault(c => c.CellReference?.Value == cellRef);
        if (cell == null) return null;

        var val = cell.CellValue?.InnerText;
        if (val == null) return null;

        // 공유 문자열 처리
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var sst = wbPart.SharedStringTablePart?.SharedStringTable;
            if (sst == null) return val;
            if (int.TryParse(val, out var sstIndex) && sstIndex >= 0 && sstIndex < sst.ChildElements.Count)
            {
                return sst.ElementAt(sstIndex).InnerText;
            }
            return val;
        }

        return val;
    }
}
