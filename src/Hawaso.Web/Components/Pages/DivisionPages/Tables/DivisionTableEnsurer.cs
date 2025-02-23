using Microsoft.Data.SqlClient;

namespace Dalbodre.Infrastructures.Cores
{
    public class DivisionTableEnsurer
    {
        private readonly string _connectionString;

        public DivisionTableEnsurer(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Divisions 테이블이 없으면 생성하는 메서드
        public void EnsureDivisionsTableExists()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // 테이블 존재 여부 확인
                SqlCommand cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' 
                    AND TABLE_NAME = 'Divisions'", connection);

                int tableCount = (int)cmdCheck.ExecuteScalar();

                if (tableCount == 0)
                {
                    // 테이블 생성
                    SqlCommand cmdCreateTable = new SqlCommand(@"
                        CREATE TABLE [dbo].[Divisions](
                            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Active] BIT NOT NULL DEFAULT ((1)),
                            [CreatedAt] DATETIMEOFFSET(7) NOT NULL,
                            [CreatedBy] NVARCHAR(255) NULL,
                            [Name] NVARCHAR(MAX) NULL
                        )", connection);

                    cmdCreateTable.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}
