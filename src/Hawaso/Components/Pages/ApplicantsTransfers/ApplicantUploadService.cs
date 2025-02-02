using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace VisualAcademy.Components.Pages.ApplicantsTransfers;

public class ApplicantUploadService
{
    public async Task<List<ApplicantTransfer>> ParseExcelAsync(Stream fileStream)
    {
        var applicants = new List<ApplicantTransfer>();

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset stream position

        using var spreadsheetDocument = SpreadsheetDocument.Open(memoryStream, false);
        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
        Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
        WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

        foreach (Row row in sheetData.Elements<Row>().Skip(7))
        {
            var cellEnumerator = row.Elements<Cell>().GetEnumerator();

            cellEnumerator.MoveNext();
            var department = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var employeeId = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var firstName = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var middleInitial = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var lastName = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var dateBirthday = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var ssNumber = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var address1 = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var address2 = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var city = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var state = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var zip = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var gender = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var cellPhone = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var primaryEmail = ReadCellValue(workbookPart, cellEnumerator.Current);

            // 이후 열들에 대한 처리...
            // ...

            cellEnumerator.MoveNext();
            var employmentStatus = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var dateSeniority = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var defaultJobsHR = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var employeeStatus = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var tribalNation = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var gamingLicenseType = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var dateRehired = ReadCellValue(workbookPart, cellEnumerator.Current);

            cellEnumerator.MoveNext();
            var dateTerminated = ReadCellValue(workbookPart, cellEnumerator.Current);

            applicants.Add(new ApplicantTransfer
            {
                DepartmentName = department,
                EmployeeId = employeeId,
                FirstName = firstName,
                MiddleName = middleInitial,
                LastName = lastName,
                DOB = dateBirthday,
                SSN = ssNumber,
                Address = address1 + " " + address2,
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
                // 추가적으로 필요한 매핑을 계속 진행...
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

    private string ReadCellValue(WorkbookPart workbookPart, Cell cell)
    {
        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(int.Parse(cell.CellValue.InnerText)).InnerText;
        }
        else
        {
            return cell.CellValue?.InnerText;
        }
    }
}
