using Microsoft.Data.SqlClient;

namespace Hawaso.Infrastructures
{
    public class TenantSchemaEnhancerCreateApplicantsTransfersTable
    {
        private string _connectionString;

        public TenantSchemaEnhancerCreateApplicantsTransfersTable(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateApplicantsTransfersTable()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlCommand cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' 
                    AND TABLE_NAME = 'ApplicantsTransfers'", connection);

                int tableCount = (int)cmdCheck.ExecuteScalar();

                if (tableCount == 0)
                {
                    SqlCommand cmdCreateTable = new SqlCommand(@"

CREATE TABLE [dbo].[ApplicantsTransfers](
    [ID] [bigint] IDENTITY(1,1) PRIMARY KEY,

    [TenantName] [nvarchar](max) NULL,

    [FirstName] [nvarchar](max) NULL,
    [LastName] [nvarchar](max) NULL,
    [MiddleName] [nvarchar](max) NULL,
    [FullName] [nvarchar](max) NULL,
    [Address] [nvarchar](max) NULL,
    [BirthCity] [nvarchar](max) NULL,
    [BirthCountry] [nvarchar](max) NULL,
    [BirthState] [nvarchar](max) NULL,
    [City] [nvarchar](max) NULL,
    [DOB] [nvarchar](max) NULL,
    [Email] [nvarchar](max) NULL,
    [Gender] [nvarchar](max) NULL,
    [CellPhone] [nvarchar](max) NULL,
    [HomePhone] [nvarchar](max) NULL,
    [WorkPhone] [nvarchar](max) NULL,
    [LicenseDate] [datetime2](7) NULL,
    [OriginalLicenseDate] [datetime2](7) NULL,
    [LicenseExpirationDate] [datetime2](7) NULL,
    [LicenseRenewalDate] [datetime2](7) NULL,
    [LicenseTypeID] [bigint] NULL,
    [PostalCode] [nvarchar](max) NULL,
    [PrimaryPhone] [nvarchar](max) NULL,
    [SecondaryPhone] [nvarchar](max) NULL,
    [State] [nvarchar](max) NULL,
    [RehireDate] [datetime2](7) NULL,
    [SSN] [nvarchar](max) NULL,
    [TerminationDate] [datetime2](7) NULL,
    [PositionID] [bigint] NULL,
    [DriverLicenseNumber] [nvarchar](max) NULL,
    [DriverLicenseState] [nvarchar](max) NULL,
    [EmploymentStatusID] [bigint] NULL,
    [LicenseNumber] [nvarchar](max) NULL,
    [LicenseStatusID] [bigint] NULL,
    [ProcessStatusID] [bigint] NULL,
    [HireDate] [datetime2](7) NULL,
    [Photo] [nvarchar](max) NULL,
    [DepartmentID] [bigint] NULL,
    [NigcClassification] [nvarchar](max) NULL,
    [StateClassification] [nvarchar](max) NULL,
    [BadgePhotoVarBinary] [nvarchar](max) NULL,
    [BadgePhotoImage] [nvarchar](max) NULL,
    [Custom1] [nvarchar](max) NULL,
    [Custom2] [nvarchar](max) NULL,
    [Custom3] [nvarchar](max) NULL,
    [Custom4] [nvarchar](max) NULL,
    [Custom5] [nvarchar](max) NULL,
    [Custom6] [nvarchar](max) NULL,
    [Confidential] [bit] NULL,
    [Group1] [bit] NULL,
    [Group2] [bit] NULL,
    [Group3] [bit] NULL,
    [InvestigationTypeID] [tinyint] NULL,
    [InvestigationType] [nvarchar](max) NULL,
    [BadgeNumber] [nvarchar](max) NULL,
    [Location] [nvarchar](max) NULL,
    [EyeColor] [nvarchar](max) NULL,
    [TribalMembershipInfo] [nvarchar](max) NULL,
    [BadgingAuthorizationNumber] [nvarchar](max) NULL,
    [BadgeDate] [datetime] NULL,
    [Citizenship] [nvarchar](max) NULL,
    [Ethnicity] [nvarchar](max) NULL,
    [SRCRRelatives] [nvarchar](max) NULL,
    [HighestEducationCompleted] [nvarchar](max) NULL,
    [FormData] [nvarchar](max) NULL,
    [DisplayName] [nvarchar](max) NULL,

    [DepartmentName] [nvarchar](max) NULL,
    [EmployeeId] [nvarchar](max) NULL,
    [Address2] [nvarchar](max) NULL,
    [ZipCode] [nvarchar](max) NULL,
    [PrimaryEmail] [nvarchar](max) NULL,
    [EmploymentStatus] [nvarchar](max) NULL,
    [DateSeniority] [nvarchar](max) NULL,
    [DefaultJobsHR] [nvarchar](max) NULL,
    [EmployeeStatus] [nvarchar](max) NULL,
    [TribalNation] [nvarchar](max) NULL,
    [GamingLicenseType] [nvarchar](max) NULL,
    [DateRehired] [nvarchar](max) NULL,
    [DateTerminated] [nvarchar](max) NULL,

    [DateCreated] DateTime NULL Default(GetDate()),
)

                    ", connection);

                    cmdCreateTable.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}
