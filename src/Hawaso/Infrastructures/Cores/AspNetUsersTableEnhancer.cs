namespace Dalbodre.Infrastructures.Cores;

public class AspNetUsersTableEnhancer
{
    private readonly string _connectionString;

    public AspNetUsersTableEnhancer(string connectionString)
    {
        _connectionString = connectionString;
    }

    // AspNetUsers 테이블에 필요한 컬럼이 없으면 추가하는 메서드
    public void EnsureColumnsExist()
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            try
            {
                connection.Open();

                // ShowInDropdown 컬럼 추가
                AddColumnIfNotExists(connection, "ShowInDropdown", "BIT NULL DEFAULT 0");

                // RefreshToken 컬럼 추가
                AddColumnIfNotExists(connection, "RefreshToken", "NVARCHAR(MAX) NULL");

                // RefreshTokenExpiryTime 컬럼 추가
                AddColumnIfNotExists(connection, "RefreshTokenExpiryTime", "DATETIME NULL");

                // DivisionId 컬럼 추가
                AddColumnIfNotExists(connection, "DivisionId", "BigInt Null Default 0");

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating AspNetUsers table: {ex.Message}");
            }
        }
    }

    // 특정 컬럼이 존재하지 않으면 추가하는 메서드
    private void AddColumnIfNotExists(SqlConnection connection, string columnName, string columnDefinition)
    {
        string query = $@"
                IF NOT EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = '{columnName}'
                ) 
                BEGIN
                    ALTER TABLE dbo.AspNetUsers ADD {columnName} {columnDefinition};
                END";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }
    }
}
