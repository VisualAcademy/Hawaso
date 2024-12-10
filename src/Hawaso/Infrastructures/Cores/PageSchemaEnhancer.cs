using Microsoft.Data.SqlClient;

namespace Dalbodre.Infrastructures.Cores
{
    public class PageSchemaEnhancer
    {
        private readonly string _connectionString;

        public PageSchemaEnhancer(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Pages 테이블이 없으면 생성하는 메서드
        public void EnsurePagesTableExists()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlCommand cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' 
                    AND TABLE_NAME = 'Pages'", connection);

                int tableCount = (int)cmdCheck.ExecuteScalar();

                if (tableCount == 0)
                {
                    SqlCommand cmdCreateTable = new SqlCommand(@"
                        CREATE TABLE [dbo].[Pages](
                            [Id] INT IDENTITY(1,1) PRIMARY KEY,
                            [TenantName] NVARCHAR(MAX) NOT NULL DEFAULT 'Kodee',
                            [PageName] VARCHAR(50) NOT NULL DEFAULT 'Contact',
                            [Title] NVARCHAR(200) NOT NULL,
                            [Content] NVARCHAR(MAX) NOT NULL,
                            [LastUpdated] DATETIME NOT NULL DEFAULT GETDATE()
                        )", connection);

                    cmdCreateTable.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        // 특정 테이블에 특정 컬럼이 없으면 추가하는 메서드
        public void AddColumnIfNotExists(string tableName, string columnName, string columnDefinition)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlCommand cmdCheck = new SqlCommand($@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = @tableName 
                        AND COLUMN_NAME = @columnName
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

        // Pages 테이블에 열을 추가하는 메서드
        public void AddCreatedByColumnIfNotExists()
        {
            AddColumnIfNotExists("Pages", "CreatedBy", "NVARCHAR(100) NULL");
        }

        public void AddLastModifiedByColumnIfNotExists()
        {
            AddColumnIfNotExists("Pages", "LastModifiedBy", "NVARCHAR(100) NULL");
        }

        public void AddIsPublishedColumnIfNotExists()
        {
            AddColumnIfNotExists("Pages", "IsPublished", "BIT NULL DEFAULT 0");
        }

        // 전체 스키마를 보장하는 메서드
        public void EnsureSchema()
        {
            EnsurePagesTableExists();
            AddCreatedByColumnIfNotExists();
            AddLastModifiedByColumnIfNotExists();
            AddIsPublishedColumnIfNotExists();
        }

        // 특정 테넌트의 IsPublished 열을 1로 설정하는 메서드
        public void PublishAllPagesForSpecificTenants()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlCommand cmdUpdate = new SqlCommand(@"
                    UPDATE dbo.Pages
                    SET IsPublished = 1
                    WHERE TenantName IN ('Kodee', 'Hawaso')", connection);

                cmdUpdate.ExecuteNonQuery();

                connection.Close();
            }
        }

        // IsPublished 열의 값이 NULL인 경우 false로 업데이트하는 메서드
        public void UpdateNullIsPublishedToFalse()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlCommand cmdUpdate = new SqlCommand(@"
                    UPDATE dbo.Pages
                    SET IsPublished = 0
                    WHERE IsPublished IS NULL", connection);

                cmdUpdate.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
