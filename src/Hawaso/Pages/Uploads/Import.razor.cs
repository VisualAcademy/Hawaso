using BlazorInputFile;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Uploads;

public partial class Import
{
    #region Injectors

    [Inject]
    public IUploadRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    [Inject]
    public IFileStorageManager FileStorageManager { get; set; } = default!;

    #endregion

    #region Properties/Fields

    public Upload Model { get; set; } = new();

    public string ParentId { get; set; } = string.Empty;

    protected int[] parentIds = { 1, 2, 3 };

    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    public List<Upload> Models { get; set; } = new();

    #endregion

    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        Model.ParentId = parentId;

        // 선택된 첫 번째 파일만 업로드
        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file is not null)
            {
                var savedName = await FileStorageManager.UploadAsync(
                    file.Data,
                    file.Name,
                    "",
                    overwrite: true);

                Model.FileName = savedName;
                Model.FileSize = Convert.ToInt32(file.Size);
            }
        }

        // 엑셀에서 파싱된 행들을 저장
        foreach (var m in Models)
        {
            m.FileName = Model.FileName;
            m.FileSize = Model.FileSize;
            m.ParentId = Model.ParentId;

            await UploadRepositoryAsyncReference.AddAsync(m);
        }

        NavigationManagerReference.NavigateTo("/Uploads");
    }

    protected async Task HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();

        if (selectedFiles.Length == 0)
        {
            Models.Clear();
            StateHasChanged();
            return;
        }

        var file = selectedFiles.FirstOrDefault();

        if (file is null)
        {
            Models.Clear();
            StateHasChanged();
            return;
        }

        using var stream = new MemoryStream();

        await file.Data.CopyToAsync(stream);
        stream.Position = 0;

        Models.Clear();

        // Open XML로 엑셀 읽기
        // 첫 번째 시트 사용, 1행은 헤더, 2행부터 데이터로 가정
        using var doc = SpreadsheetDocument.Open(stream, false);

        var wbPart = doc.WorkbookPart;

        if (wbPart?.Workbook?.Sheets is null)
        {
            StateHasChanged();
            return;
        }

        var firstSheet = wbPart.Workbook.Sheets
            .Elements<Sheet>()
            .FirstOrDefault();

        var relationshipId = firstSheet?.Id?.Value;

        if (string.IsNullOrWhiteSpace(relationshipId))
        {
            StateHasChanged();
            return;
        }

        if (wbPart.GetPartById(relationshipId) is not WorksheetPart wsPart)
        {
            StateHasChanged();
            return;
        }

        var worksheet = wsPart.Worksheet;

        if (worksheet is null)
        {
            StateHasChanged();
            return;
        }

        var rows = worksheet.Descendants<Row>();

        foreach (var row in rows.Skip(1))
        {
            var rowIndex = (int)(row.RowIndex?.Value ?? 0);

            if (rowIndex == 0)
            {
                continue;
            }

            // A열: Name, B열: DownCount
            var name = ReadCellString(wbPart, worksheet, $"A{rowIndex}")?.Trim() ?? string.Empty;
            var downText = ReadCellString(wbPart, worksheet, $"B{rowIndex}")?.Trim() ?? "0";

            int downCount = 0;

            // 정수 또는 소수 형태로 저장된 숫자 모두 처리
            if (!int.TryParse(downText, NumberStyles.Any, CultureInfo.InvariantCulture, out downCount))
            {
                if (double.TryParse(downText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDouble))
                {
                    downCount = (int)Math.Round(parsedDouble);
                }
            }

            // 이름도 없고 다운로드 수도 0이면 빈 행으로 보고 제외
            if (string.IsNullOrWhiteSpace(name) && downCount == 0)
            {
                continue;
            }

            Models.Add(new Upload
            {
                Name = name,
                DownCount = downCount
            });
        }

        StateHasChanged();
    }

    #region Open XML Helpers

    private static string? ReadCellString(
        WorkbookPart wbPart,
        Worksheet worksheet,
        string cellRef)
    {
        var cell = worksheet
            .Descendants<Cell>()
            .FirstOrDefault(c => c.CellReference?.Value == cellRef);

        if (cell is null)
        {
            return null;
        }

        var value = cell.CellValue?.InnerText;

        if (value is null)
        {
            return null;
        }

        // 공유 문자열 처리
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var sharedStringTable = wbPart.SharedStringTablePart?.SharedStringTable;

            if (sharedStringTable is null)
            {
                return value;
            }

            if (int.TryParse(value, out var sharedStringIndex)
                && sharedStringIndex >= 0
                && sharedStringIndex < sharedStringTable.ChildElements.Count)
            {
                return sharedStringTable.ElementAt(sharedStringIndex).InnerText;
            }

            return value;
        }

        return value;
    }

    #endregion
}