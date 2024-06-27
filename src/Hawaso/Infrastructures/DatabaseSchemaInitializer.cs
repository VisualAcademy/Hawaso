using Microsoft.Data.SqlClient;

namespace VisualAcademy.Infrastructures;

// 데이터베이스 스키마 초기화 및 확장을 관리하는 클래스입니다.
public class DatabaseSchemaInitializer(string defaultConnectionString)
{
    // 스키마를 초기화하거나 확장하는 메서드입니다.
    public void InitializeOrEnhanceSchema()
    {
        // 필요한 열이 존재하지 않는 경우 추가합니다.
        EnsureColumnExists("Incidents", "SurveillanceName", "nvarchar(max) NULL");
    }

    // 지정된 테이블에 특정 열이 존재하지 않는 경우 추가하는 메서드입니다.
    private void EnsureColumnExists(string tableName, string columnName, string columnDefinition)
    {
        using (SqlConnection connection = new SqlConnection(defaultConnectionString))
        {
            connection.Open(); // 데이터베이스 연결을 엽니다.

            // 열이 존재하는지 확인하는 SQL 명령문입니다.
            SqlCommand cmdCheck = new SqlCommand($@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName
                    ) 
                    BEGIN
                        ALTER TABLE dbo.{tableName} ADD {columnName} {columnDefinition}; // 열이 없으면 추가합니다.
                    END", connection);

            cmdCheck.Parameters.AddWithValue("@tableName", tableName); // 테이블 이름을 파라미터로 추가합니다.
            cmdCheck.Parameters.AddWithValue("@columnName", columnName); // 열 이름을 파라미터로 추가합니다.

            cmdCheck.ExecuteNonQuery(); // SQL 명령문을 실행합니다.

            connection.Close(); // 데이터베이스 연결을 닫습니다.
        }
    }
}
