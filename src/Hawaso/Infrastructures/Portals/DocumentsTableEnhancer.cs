using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Hawaso.Infrastructures
{
    public class DocumentsTableEnhancer
    {
        private readonly string _connectionString;

        public DocumentsTableEnhancer(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 특정 테이블에 특정 컬럼이 없으면 추가하는 메서드
        /// </summary>
        public async Task EnsureColumnExistsAsync(string tableName, string columnName, string columnDefinition, string defaultValue = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var commandText = $@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName
                    )
                    BEGIN
                        ALTER TABLE dbo.{tableName} ADD {columnName} {columnDefinition} 
                        {(defaultValue != null ? $"DEFAULT '{defaultValue}'" : "")};
                    END";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@tableName", tableName);
                    command.Parameters.AddWithValue("@columnName", columnName);
                    await command.ExecuteNonQueryAsync();
                }

                connection.Close();
            }
        }

        /// <summary>
        /// OwnershipType 컬럼의 값이 NULL이면 'Employees'로 업데이트하는 메서드
        /// </summary>
        public async Task UpdateNullOwnershipTypeAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var updateCommandText = @"
                    UPDATE dbo.Documents
                    SET OwnershipType = 'Employees'
                    WHERE OwnershipType IS NULL";

                using (var command = new SqlCommand(updateCommandText, connection))
                {
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"{rowsAffected}개의 NULL 값을 'Employees'로 업데이트하였습니다.");
                }

                connection.Close();
            }
        }

        /// <summary>
        /// Documents 테이블에 필요한 컬럼이 없으면 추가하고, NULL 값이 있으면 기본값을 설정하는 메서드
        /// </summary>
        public async Task EnsureDocumentsTableColumnsAsync()
        {
            await EnsureColumnExistsAsync("Documents", "OwnershipType", "NVARCHAR(255) NULL", "Employees");
            //await UpdateNullOwnershipTypeAsync(); // NULL 값이 있으면 'Employees'로 업데이트
        }
    }
}
