using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Web.Infrastructures.Assets.Tenants
{
    public class TenantSchemaEnhancerEnsureManufacturersTable
    {
        private readonly string _masterConnectionString;
        private readonly ILogger<TenantSchemaEnhancerEnsureManufacturersTable> _logger;

        public TenantSchemaEnhancerEnsureManufacturersTable(
            string masterConnectionString,
            ILogger<TenantSchemaEnhancerEnsureManufacturersTable> logger)
        {
            _masterConnectionString = masterConnectionString;
            _logger = logger;
        }

        public void EnhanceTenantDatabases()
        {
            var tenantConnectionStrings = GetTenantConnectionStrings();

            foreach (var connStr in tenantConnectionStrings)
            {
                try
                {
                    EnsureManufacturersTable(connStr);
                    _logger.LogInformation($"Manufacturers table processed (tenant DB): {connStr}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{connStr}] Error processing tenant DB");
                }
            }
        }

        public void EnhanceMasterDatabase()
        {
            try
            {
                EnsureManufacturersTable(_masterConnectionString);
                _logger.LogInformation("Manufacturers table processed (master DB)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing master DB");
            }
        }

        private List<string> GetTenantConnectionStrings()
        {
            var result = new List<string>();

            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("SELECT ConnectionString FROM dbo.Tenants", connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var connectionString = reader["ConnectionString"]?.ToString();
                        if (!string.IsNullOrEmpty(connectionString))
                        {
                            result.Add(connectionString);
                        }
                    }
                }
            }

            return result;
        }

        private void EnsureManufacturersTable(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'Manufacturers'", connection);

                int tableCount = (int)cmdCheck.ExecuteScalar();

                if (tableCount == 0)
                {
                    var cmdCreate = new SqlCommand(@"
                        CREATE TABLE [dbo].[Manufacturers] (
                            [Id] INT NOT NULL PRIMARY KEY Identity(1, 1),
                            Name NVarChar(255) Not Null,
                            ManufacturerCode NVarChar(255) Null,
                            Comment NVarChar(Max) Null
                        )", connection);

                    cmdCreate.ExecuteNonQuery();

                    _logger.LogInformation("Manufacturers table created.");
                }
                else
                {
                    var expectedColumns = new Dictionary<string, string>
                    {
                        ["Name"] = "NVarChar(255) Not Null",
                        ["ManufacturerCode"] = "NVarChar(255) Null",
                        ["Comment"] = "NVarChar(Max) Null"
                    };

                    foreach (var kvp in expectedColumns)
                    {
                        var columnName = kvp.Key;
                        var columnType = kvp.Value;

                        var cmdColumnCheck = new SqlCommand(@"
                            SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = 'Manufacturers' AND COLUMN_NAME = @ColumnName", connection);
                        cmdColumnCheck.Parameters.AddWithValue("@ColumnName", columnName);

                        int colExists = (int)cmdColumnCheck.ExecuteScalar();

                        if (colExists == 0)
                        {
                            var alterCmd = new SqlCommand(
                                $"ALTER TABLE [dbo].[Manufacturers] ADD [{columnName}] {columnType}", connection);
                            alterCmd.ExecuteNonQuery();

                            _logger.LogInformation($"Column added: {columnName} ({columnType})");
                        }
                    }
                }

                // 테이블이 비어있으면 기본값 2건 추가
                var cmdCountRows = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Manufacturers]", connection);
                int rowCount = (int)cmdCountRows.ExecuteScalar();

                if (rowCount == 0)
                {
                    var cmdInsertDefaults = new SqlCommand(@"
                        INSERT INTO [dbo].[Manufacturers] (Name, ManufacturerCode, Comment)
                        VALUES 
                            (N'Manufacturer 1', N'MANU-001', N'Default manufacturer 1'),
                            (N'Manufacturer 2', N'MANU-002', N'Default manufacturer 2')", connection);

                    int inserted = cmdInsertDefaults.ExecuteNonQuery();
                    _logger.LogInformation($"Manufacturers 기본 데이터 {inserted}건 삽입 완료");
                }
            }
        }

        public static void Run(IServiceProvider services, bool forMaster)
        {
            try
            {
                var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureManufacturersTable>>();
                var config = services.GetRequiredService<IConfiguration>();
                var masterConnectionString = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(masterConnectionString))
                {
                    throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");
                }

                var enhancer = new TenantSchemaEnhancerEnsureManufacturersTable(masterConnectionString, logger);

                if (forMaster)
                {
                    enhancer.EnhanceMasterDatabase();
                }
                else
                {
                    enhancer.EnhanceTenantDatabases();
                }
            }
            catch (Exception ex)
            {
                var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureManufacturersTable>>();
                fallbackLogger?.LogError(ex, "Error while processing Manufacturers table.");
            }
        }
    }
}
