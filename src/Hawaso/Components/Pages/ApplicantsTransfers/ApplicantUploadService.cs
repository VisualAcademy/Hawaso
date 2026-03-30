using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;

namespace VisualAcademy.Components.Pages.ApplicantsTransfers;

public class ApplicantUploadService
{
    public async Task<List<ApplicantTransfer>> ParseExcelAsync(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        var applicants = new List<ApplicantTransfer>();

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var spreadsheetDocument = SpreadsheetDocument.Open(memoryStream, false);

        var workbookPart = spreadsheetDocument.WorkbookPart
            ?? throw new InvalidOperationException("The Excel file does not contain a workbook part.");

        var workbook = workbookPart.Workbook
            ?? throw new InvalidOperationException("The Excel file does not contain a workbook.");

        var sheet = workbook.Descendants<Sheet>().FirstOrDefault()
            ?? throw new InvalidOperationException("The Excel file does not contain any sheets.");

        var sheetId = sheet.Id?.Value;
        if (string.IsNullOrWhiteSpace(sheetId))
        {
            throw new InvalidOperationException("The sheet Id is missing.");
        }

        var worksheetPart = workbookPart.GetPartById(sheetId) as WorksheetPart
            ?? throw new InvalidOperationException("The worksheet part could not be found.");

        var worksheet = worksheetPart.Worksheet
            ?? throw new InvalidOperationException("The worksheet could not be loaded.");

        var sheetData = worksheet.Elements<SheetData>().FirstOrDefault()
            ?? throw new InvalidOperationException("The worksheet does not contain sheet data.");

        foreach (var row in sheetData.Elements<Row>().Skip(7))
        {
            string? department = GetCellValueByColumnName(workbookPart, row, "A");
            string? employeeId = GetCellValueByColumnName(workbookPart, row, "B");
            string? firstName = GetCellValueByColumnName(workbookPart, row, "C");
            string? middleInitial = GetCellValueByColumnName(workbookPart, row, "D");
            string? lastName = GetCellValueByColumnName(workbookPart, row, "E");
            string? dateBirthday = GetCellValueByColumnName(workbookPart, row, "F");
            string? ssNumber = GetCellValueByColumnName(workbookPart, row, "G");
            string? address1 = GetCellValueByColumnName(workbookPart, row, "H");
            string? address2 = GetCellValueByColumnName(workbookPart, row, "I");
            string? city = GetCellValueByColumnName(workbookPart, row, "J");
            string? state = GetCellValueByColumnName(workbookPart, row, "K");
            string? zip = GetCellValueByColumnName(workbookPart, row, "L");
            string? gender = GetCellValueByColumnName(workbookPart, row, "M");
            string? cellPhone = GetCellValueByColumnName(workbookPart, row, "N");
            string? primaryEmail = GetCellValueByColumnName(workbookPart, row, "O");

            // 실제 엑셀 구조에 맞게 열 문자를 조정하세요.
            // 아래는 기존 코드 흐름을 유지한 예시입니다.
            string? employmentStatus = GetCellValueByColumnName(workbookPart, row, "P");
            string? dateSeniority = GetCellValueByColumnName(workbookPart, row, "Q");
            string? defaultJobsHR = GetCellValueByColumnName(workbookPart, row, "R");
            string? employeeStatus = GetCellValueByColumnName(workbookPart, row, "S");
            string? tribalNation = GetCellValueByColumnName(workbookPart, row, "T");
            string? gamingLicenseType = GetCellValueByColumnName(workbookPart, row, "U");
            string? dateRehired = GetCellValueByColumnName(workbookPart, row, "V");
            string? dateTerminated = GetCellValueByColumnName(workbookPart, row, "W");

            // 완전히 빈 행은 건너뛰기
            if (IsRowEmpty(
                department, employeeId, firstName, middleInitial, lastName,
                dateBirthday, ssNumber, address1, address2, city, state, zip,
                gender, cellPhone, primaryEmail, employmentStatus, dateSeniority,
                defaultJobsHR, employeeStatus, tribalNation, gamingLicenseType,
                dateRehired, dateTerminated))
            {
                continue;
            }

            applicants.Add(new ApplicantTransfer
            {
                DepartmentName = department,
                EmployeeId = employeeId,
                FirstName = firstName,
                MiddleName = middleInitial,
                LastName = lastName,
                DOB = dateBirthday,
                SSN = ssNumber,
                Address = $"{address1} {address2}".Trim(),
                City = city,
                State = state,
                PostalCode = zip,
                Gender = gender,
                CellPhone = cellPhone,
                HomePhone = cellPhone,
                SecondaryPhone = cellPhone,
                WorkPhone = cellPhone,
                Email = primaryEmail,
                PrimaryEmail = primaryEmail,
                EmploymentStatus = employmentStatus,
                DateSeniority = dateSeniority,
                DefaultJobsHR = defaultJobsHR,
                EmployeeStatus = employeeStatus,
                TribalNation = tribalNation,
                GamingLicenseType = gamingLicenseType,
                DateRehired = dateRehired,
                DateTerminated = dateTerminated,
            });
        }

        return applicants;
    }

    private static string? GetCellValueByColumnName(WorkbookPart workbookPart, Row row, string columnName)
    {
        ArgumentNullException.ThrowIfNull(workbookPart);
        ArgumentNullException.ThrowIfNull(row);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var cell = row.Elements<Cell>()
            .FirstOrDefault(c =>
            {
                var cellReference = c.CellReference?.Value;
                if (string.IsNullOrWhiteSpace(cellReference))
                {
                    return false;
                }

                return string.Equals(
                    GetColumnName(cellReference),
                    columnName,
                    StringComparison.OrdinalIgnoreCase);
            });

        return ReadCellValue(workbookPart, cell);
    }

    private static string GetColumnName(string cellReference)
    {
        var columnName = new StringBuilder();

        foreach (char ch in cellReference)
        {
            if (char.IsLetter(ch))
            {
                columnName.Append(ch);
            }
            else
            {
                break;
            }
        }

        return columnName.ToString();
    }

    private static string? ReadCellValue(WorkbookPart workbookPart, Cell? cell)
    {
        if (cell is null)
        {
            return null;
        }

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
            if (sharedStringTable is null)
            {
                return null;
            }

            if (!int.TryParse(cell.CellValue?.InnerText, out int sharedStringIndex))
            {
                return null;
            }

            return sharedStringTable.Elements<SharedStringItem>()
                .ElementAtOrDefault(sharedStringIndex)?
                .InnerText;
        }

        return cell.CellValue?.InnerText;
    }

    private static bool IsRowEmpty(params string?[] values)
    {
        return values.All(string.IsNullOrWhiteSpace);
    }
}