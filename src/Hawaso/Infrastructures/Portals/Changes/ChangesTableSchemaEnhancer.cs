using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Azunt.Infrastructures
{
    public class ChangesTableSchemaEnhancer
    {
        private readonly string _connectionString;
        private readonly ILogger<ChangesTableSchemaEnhancer>? _logger;

        public ChangesTableSchemaEnhancer(string connectionString, ILogger<ChangesTableSchemaEnhancer>? logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public void EnhanceChangesTable()
        {
            EnsureChangesTableExists();
            AddColumnIfNotExists("Changes", "PostalCode", "NVARCHAR(35) NULL");
            AddColumnIfNotExists("Changes", "State", "NVARCHAR(2) NULL");
            AddColumnIfNotExists("Changes", "City", "NVARCHAR(70) NULL");
        }

        private void EnsureChangesTableExists()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
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
                            CREATE TABLE [dbo].[Changes] (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Email NVARCHAR(255) NULL,
                                UserName NVARCHAR(255) NULL,
                                PhoneNumber NVARCHAR(50) NULL,
                                Address NVARCHAR(255) NULL,
                                SecondaryPhone NVARCHAR(255) NULL,
                                MobilePhone NVARCHAR(50) NULL,
                                FirstName NVARCHAR(255) NULL,
                                MiddleName NVARCHAR(255) NULL,
                                LastName NVARCHAR(255) NULL,
                                Age INT NULL,
                                IsComplete BIT NULL,
                                IsActive BIT NULL,
                                CreatedAt DATETIME DEFAULT GETDATE() NULL,
                                TenantName NVARCHAR(255) NULL,
                                SSN NVARCHAR(255) NULL,
                                CriminalHistory NVARCHAR(MAX) NULL,
                                PrimaryPhone NVARCHAR(255) NULL,
                                PhysicalAddress NVARCHAR(255) NULL,
                                MailingAddress NVARCHAR(255) NULL,
                                NewEmail NVARCHAR(255) NULL,
                                BadgeName NVARCHAR(255) NULL,
                                ReasonForChange NVARCHAR(MAX) NULL,
                                MaritalStatus NVARCHAR(50) NULL,
                                SpousesName NVARCHAR(100) NULL,
                                RoommateName1 NVARCHAR(255) NULL,
                                RoommateName2 NVARCHAR(255) NULL,
                                RelationshipDisclosureName NVARCHAR(100) NULL,
                                RelationshipDisclosurePosition NVARCHAR(100) NULL,
                                RelationshipDisclosure NVARCHAR(MAX) NULL,
                                AdditionalEmploymentBusinessName NVARCHAR(255) NULL,
                                AdditionalEmploymentStartDate DATE NULL,
                                AdditionalEmploymentEndDate DATE NULL,
                                AdditionalEmploymentLocation NVARCHAR(255) NULL,
                                PostalCode NVARCHAR(35) NULL,
                                State NVARCHAR(2) NULL,
                                City NVARCHAR(70) NULL
                            )", connection);

                        cmdCreateTable.ExecuteNonQuery();
                        _logger?.LogInformation("Changes 테이블을 생성했습니다.");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Changes 테이블 확인 중 오류 발생");
                }
            }
        }

        private void AddColumnIfNotExists(string tableName, string columnName, string columnDefinition)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
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
                    _logger?.LogInformation($"{columnName} 컬럼을 추가했습니다.");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"{columnName} 컬럼 추가 중 오류 발생");
                }
            }
        }
    }
}
