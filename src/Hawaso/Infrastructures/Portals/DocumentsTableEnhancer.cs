using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Hawaso.Infrastructures
{
    public class DocumentsTableEnhancer
    {
        private readonly string _connectionString;

        public DocumentsTableEnhancer(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// 특정 테이블에 특정 컬럼이 없으면 추가하는 메서드
        /// </summary>
        public async Task EnsureColumnExistsAsync(
            string tableName,
            string columnName,
            string columnDefinition,
            string? defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name is required.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name is required.", nameof(columnName));

            if (string.IsNullOrWhiteSpace(columnDefinition))
                throw new ArgumentException("Column definition is required.", nameof(columnDefinition));

            ValidateSqlIdentifier(tableName, nameof(tableName));
            ValidateSqlIdentifier(columnName, nameof(columnName));

            string safeTableName = EscapeSqlIdentifier(tableName);
            string safeColumnName = EscapeSqlIdentifier(columnName);
            string defaultClause = BuildDefaultClause(defaultValue);

            var commandText = $@"
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = @tableName
      AND COLUMN_NAME = @columnName
)
BEGIN
    ALTER TABLE [dbo].{safeTableName}
    ADD {safeColumnName} {columnDefinition}{defaultClause};
END";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(commandText, connection);
            command.Parameters.AddWithValue("@tableName", tableName);
            command.Parameters.AddWithValue("@columnName", columnName);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// OwnershipType 컬럼의 값이 NULL이면 'Employees'로 업데이트하는 메서드
        /// </summary>
        public async Task UpdateNullOwnershipTypeAsync()
        {
            const string updateCommandText = @"
UPDATE [dbo].[Documents]
SET [OwnershipType] = @ownershipType
WHERE [OwnershipType] IS NULL;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(updateCommandText, connection);
            command.Parameters.AddWithValue("@ownershipType", "Employees");

            int rowsAffected = await command.ExecuteNonQueryAsync();
            Console.WriteLine($"{rowsAffected}개의 NULL 값을 'Employees'로 업데이트하였습니다.");
        }

        /// <summary>
        /// Documents 테이블에 필요한 컬럼이 없으면 추가하고, NULL 값이 있으면 기본값을 설정하는 메서드
        /// </summary>
        public async Task EnsureDocumentsTableColumnsAsync()
        {
            await EnsureColumnExistsAsync(
                "Documents",
                "OwnershipType",
                "NVARCHAR(255) NULL",
                "Employees");

            // 기존 NULL 데이터까지 채우려면 활성화
            // await UpdateNullOwnershipTypeAsync();
        }

        private static void ValidateSqlIdentifier(string identifier, string paramName)
        {
            if (!Regex.IsMatch(identifier, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                throw new ArgumentException(
                    $"Invalid SQL identifier: {identifier}",
                    paramName);
            }
        }

        private static string EscapeSqlIdentifier(string identifier)
        {
            return $"[{identifier.Replace("]", "]]")}]";
        }

        private static string BuildDefaultClause(string? defaultValue)
        {
            if (defaultValue is null)
            {
                return string.Empty;
            }

            string escapedValue = defaultValue.Replace("'", "''");
            return $" DEFAULT ('{escapedValue}')";
        }
    }
}