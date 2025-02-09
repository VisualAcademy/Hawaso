namespace Hawaso.Infrastructures.Reports;

public class DailyLogsTableEnhancer(string connectionString)
{
    private readonly string _connectionString = connectionString;

    // 테이블이 없으면 생성하고, 컬럼이 없으면 추가하는 메서드
    public void EnsureDailyLogsTable()
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            try
            {
                connection.Open();

                // 1. DailyLogs 테이블이 없으면 생성
                CreateDailyLogsTableIfNotExists(connection);

                // 2. DailyLogs 테이블에 필요한 컬럼이 없으면 추가
                AddColumnIfNotExists(connection, "DivisionId", "BIGINT NULL DEFAULT 0");
                AddColumnIfNotExists(connection, "DivisionName", "NVARCHAR(255) NULL");

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating DailyLogs table: {ex.Message}");
            }
        }
    }

    // 특정 테이블이 존재하는지 확인 후 생성하는 메서드
    private void CreateDailyLogsTableIfNotExists(SqlConnection connection)
    {
        string checkTableQuery = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' 
                AND TABLE_NAME = 'DailyLogs'";

        using (SqlCommand cmdCheck = new SqlCommand(checkTableQuery, connection))
        {
            int tableCount = (int)cmdCheck.ExecuteScalar();
            if (tableCount == 0)
            {
                string createTableQuery = @"
                        CREATE TABLE [dbo].[DailyLogs] (
                            [ID]                INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [ParentKey]         NVARCHAR(255)   NULL,  
                            [LogNumber]         NVARCHAR(50)    NULL,
                            [Occurred]          SMALLDATETIME   NULL,
                            [Ended]             SMALLDATETIME   NULL,
                            [PropertyID]        INT             NULL,
                            [LocationID]        INT             NULL,
                            [SublocationID]     INT             NULL,
                            [DepartmentID]      INT             NULL,
                            [TopicID]           INT             NULL,
                            [Action]            NVARCHAR(MAX)   NULL,
                            [Synopsis]          NVARCHAR(MAX)   NULL,
                            [StatusID]          INT             NULL,
                            [CreatedBy]         NVARCHAR(255)   NULL,
                            [CreatedDate]       DATETIMEOFFSET  NULL,
                            [ModifiedBy]        NVARCHAR(255)   NULL,
                            [ModifiedDate]      DATETIMEOFFSET  NULL,
                            [Active]            BIT             NULL,
                            [Reference]         NVARCHAR(MAX)   NULL,
                            [Topic]             NVARCHAR(MAX)   NULL,
                            [SecondaryOperator] NVARCHAR(255)   NULL,
                            [LogDetails]        NVARCHAR(MAX)   NULL,
                            [DispatchCallID]    INT             NULL,
                            [InvestigatorID]    INT             NULL,
                            [Amount]            MONEY           NULL,
                            [Attachment]        NVARCHAR(50)    NULL,
                            [Status]            NVARCHAR(50)    NULL,
                            [Property]          NVARCHAR(50)    NULL,
                            [Location]          NVARCHAR(50)    NULL,
                            [Sublocation]       NVARCHAR(50)    NULL,
                            [Agent]             NVARCHAR(50)    NULL,
                            [Adjusted]          BIT             NULL,
                            [Sgc]               BIT             NULL,
                            [Surveillance]      BIT             NULL,
                            [Security]          BIT             NULL
                        )";

                using (SqlCommand cmdCreateTable = new SqlCommand(createTableQuery, connection))
                {
                    cmdCreateTable.ExecuteNonQuery();
                }
            }
        }
    }

    // 특정 컬럼이 존재하지 않으면 추가하는 메서드
    private void AddColumnIfNotExists(SqlConnection connection, string columnName, string columnDefinition)
    {
        string query = $@"
                IF NOT EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'DailyLogs' AND COLUMN_NAME = '{columnName}'
                ) 
                BEGIN
                    ALTER TABLE dbo.DailyLogs ADD {columnName} {columnDefinition};
                END";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }
    }
}
