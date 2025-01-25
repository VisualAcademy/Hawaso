namespace Portals.Infrastructures.Portals.Changes;

public class ChangesTableSchemaEnhancer
{
    private readonly string _connectionString;

    public ChangesTableSchemaEnhancer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void CreateChangesTable()
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            SqlCommand cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' 
                    AND TABLE_NAME = 'Changes'", connection);

            int tableCount = (int)cmdCheck.ExecuteScalar();

            if (tableCount == 0)
            {
                SqlCommand cmdCreateTable = new SqlCommand(@"
                        CREATE TABLE [dbo].[Changes](
                            [Id] INT IDENTITY(1,1) PRIMARY KEY,
                            [Email] NVARCHAR(255) NULL,
                            [UserName] NVARCHAR(255) NULL,
                            [PhoneNumber] NVARCHAR(50) NULL,
                            [Address] NVARCHAR(255) NULL,
                            [MobilePhone] NVARCHAR(50) NULL,
                            [FirstName] NVARCHAR(255) NULL,
                            [LastName] NVARCHAR(255) NULL,
                            [Age] INT NULL,
                            [IsComplete] BIT NULL,
                            [IsActive] BIT NULL,
                            [CreatedAt] DATETIME DEFAULT GETDATE() NULL
                        )", connection);

                cmdCreateTable.ExecuteNonQuery();
            }

            connection.Close();
        }
    }

    public void AddTenantNameColumnIfNotExists()
    {
        // 기존 컬럼 추가
        AddColumnIfNotExists("Changes", "TenantName", "NVARCHAR(255) NULL");
        AddColumnIfNotExists("Changes", "MiddleName", "NVARCHAR(255) NULL");
        AddColumnIfNotExists("Changes", "SecondaryPhone", "NVARCHAR(255) NULL");
        AddColumnIfNotExists("Changes", "SSN", "NVARCHAR(255) NULL");
        AddColumnIfNotExists("Changes", "CriminalHistory", "NVARCHAR(MAX) NULL");

        // 새로 추가 요청된 컬럼들
        AddColumnIfNotExists("Changes", "PrimaryPhone", "NVARCHAR(255) NULL"); // Home Phone (KodeeOne)
        AddColumnIfNotExists("Changes", "PhysicalAddress", "NVARCHAR(255) NULL"); // 물리적 주소
        AddColumnIfNotExists("Changes", "MailingAddress", "NVARCHAR(255) NULL"); // 우편 주소

        AddColumnIfNotExists("Changes", "NewEmail", "NVARCHAR(255) NULL"); // 새로운 이메일
        AddColumnIfNotExists("Changes", "BadgeName", "NVARCHAR(255) NULL");
        AddColumnIfNotExists("Changes", "ReasonForChange", "NVARCHAR(MAX) NULL"); // 변경 이유

        AddColumnIfNotExists("Changes", "MaritalStatus", "NVARCHAR(50) NULL"); // 결혼 상태
        AddColumnIfNotExists("Changes", "SpousesName", "NVARCHAR(100) NULL"); // 배우자 이름

        AddColumnIfNotExists("Changes", "RoommateName1", "NVARCHAR(255) NULL");
        AddColumnIfNotExists("Changes", "RoommateName2", "NVARCHAR(255) NULL");

        AddColumnIfNotExists("Changes", "RelationshipDisclosureName", "NVARCHAR(100) NULL"); // 관계 공개: 이름
        AddColumnIfNotExists("Changes", "RelationshipDisclosurePosition", "NVARCHAR(100) NULL"); // 관계 공개: 직위
        AddColumnIfNotExists("Changes", "RelationshipDisclosure", "NVARCHAR(MAX) NULL"); // 관계 공개: 설명

        AddColumnIfNotExists("Changes", "AdditionalEmploymentBusinessName", "NVARCHAR(255) NULL"); // 추가 고용: 회사 이름
        AddColumnIfNotExists("Changes", "AdditionalEmploymentStartDate", "DATE NULL"); // 추가 고용: 시작 날짜
        AddColumnIfNotExists("Changes", "AdditionalEmploymentEndDate", "DATE NULL"); // 추가 고용: 종료 날짜
        AddColumnIfNotExists("Changes", "AdditionalEmploymentLocation", "NVARCHAR(255) NULL"); // 추가 고용: 위치
    }

    private void AddColumnIfNotExists(string tableName, string columnName, string columnDefinition)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            SqlCommand cmdCheck = new SqlCommand($@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName
                    ) 
                    BEGIN
                        ALTER TABLE dbo.{tableName} ADD {columnName} {columnDefinition};
                    END", connection);

            cmdCheck.Parameters.AddWithValue("@tableName", tableName);
            cmdCheck.Parameters.AddWithValue("@columnName", columnName);

            cmdCheck.ExecuteNonQuery();

            connection.Close();
        }
    }
}
