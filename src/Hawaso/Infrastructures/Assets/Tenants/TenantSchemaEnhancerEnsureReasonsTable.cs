using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Web.Infrastructures.Assets.Tenants;

public class TenantSchemaEnhancerEnsureReasonsTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureReasonsTable> _logger;

    public TenantSchemaEnhancerEnsureReasonsTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureReasonsTable> logger)
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
                EnsureReasonsTable(connStr);
                _logger.LogInformation($"Reasons table processed (tenant DB): {connStr}");
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
            EnsureReasonsTable(_masterConnectionString);
            _logger.LogInformation("Reasons table processed (master DB)");
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

    private void EnsureReasonsTable(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            var cmdCheck = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'Reasons'", connection);

            int tableCount = (int)cmdCheck.ExecuteScalar();

            if (tableCount == 0)
            {
                var cmdCreate = new SqlCommand(@"
                    CREATE TABLE [dbo].[Reasons] (
                        [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Active] BIT DEFAULT ((1)) NOT NULL,
                        [CreatedAt] DATETIMEOFFSET(7) NOT NULL,
                        [CreatedBy] NVARCHAR(255) NULL,
                        [Name] NVARCHAR(MAX) NULL
                    )", connection);

                cmdCreate.ExecuteNonQuery();

                _logger.LogInformation("Reasons table created.");
            }
            else
            {
                var expectedColumns = new Dictionary<string, string>
                {
                    ["Active"] = "BIT",
                    ["CreatedAt"] = "DATETIMEOFFSET(7)",
                    ["CreatedBy"] = "NVARCHAR(255)",
                    ["Name"] = "NVARCHAR(MAX)"
                };

                foreach (var kvp in expectedColumns)
                {
                    var columnName = kvp.Key;

                    var cmdColumnCheck = new SqlCommand(@"
                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Reasons' AND COLUMN_NAME = @ColumnName", connection);
                    cmdColumnCheck.Parameters.AddWithValue("@ColumnName", columnName);

                    int colExists = (int)cmdColumnCheck.ExecuteScalar();

                    if (colExists == 0)
                    {
                        var alterCmd = new SqlCommand(
                            $"ALTER TABLE [dbo].[Reasons] ADD [{columnName}] {kvp.Value} NULL", connection);
                        alterCmd.ExecuteNonQuery();

                        _logger.LogInformation($"Column added: {columnName} ({kvp.Value})");
                    }
                }
            }

            // 테이블이 비어있으면 초기값 2건 삽입
            var cmdCountRows = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Reasons]", connection);
            int rowCount = (int)cmdCountRows.ExecuteScalar();

            if (rowCount == 0)
            {
                var cmdInsertDefaults = new SqlCommand(@"
                    INSERT INTO [dbo].[Reasons] (Active, CreatedAt, CreatedBy, Name)
                    VALUES
                        (1, SYSDATETIMEOFFSET(), 'System', 'Initial Reason 1'),
                        (1, SYSDATETIMEOFFSET(), 'System', 'Initial Reason 2')", connection);

                int inserted = cmdInsertDefaults.ExecuteNonQuery();
                _logger.LogInformation($"Reasons 기본 데이터 {inserted}건 삽입 완료");
            }
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureReasonsTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(masterConnectionString))
            {
                throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");
            }

            var enhancer = new TenantSchemaEnhancerEnsureReasonsTable(masterConnectionString, logger);

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
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureReasonsTable>>();
            fallbackLogger?.LogError(ex, "Error while processing Reasons table.");
        }
    }
}
