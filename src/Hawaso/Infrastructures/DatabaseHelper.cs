using Microsoft.Data.SqlClient;

namespace VisualAcademy;

public class DatabaseHelper
{
    public static async Task AddOrUpdateRegistrationDate(string connectionString)
    {
        using (var con = new SqlConnection(connectionString))
        {
            await con.OpenAsync();

            // 컬럼 추가
            var alterCmd = new SqlCommand
            {
                Connection = con,
                CommandText = @"
                        IF NOT EXISTS (
                            SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = N'AspNetUsers' AND COLUMN_NAME = N'RegistrationDate'
                        )
                        BEGIN
                            ALTER TABLE AspNetUsers ADD RegistrationDate DATETIMEOFFSET NULL;
                        END;
                    ",
                CommandType = System.Data.CommandType.Text
            };
            await alterCmd.ExecuteNonQueryAsync();

            // 업데이트
            var updateCmd = new SqlCommand
            {
                Connection = con,
                CommandText = @"
                        UPDATE AspNetUsers
                        SET RegistrationDate = SYSDATETIMEOFFSET()
                        WHERE RegistrationDate IS NULL;
                    ",
                CommandType = System.Data.CommandType.Text
            };
            await updateCmd.ExecuteNonQueryAsync();
        }
    }
}
