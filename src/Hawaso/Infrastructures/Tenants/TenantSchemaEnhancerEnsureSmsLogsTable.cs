using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Azunt.Infrastructures.Tenants;

public class TenantSchemaEnhancerEnsureSmsLogsTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureSmsLogsTable> _logger;

    public TenantSchemaEnhancerEnsureSmsLogsTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureSmsLogsTable> logger)
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
                EnsureSmsLogsTable(connStr);
                _logger.LogInformation($"SmsLogs table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing SmsLogs table");
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureSmsLogsTable(_masterConnectionString);
            _logger.LogInformation("SmsLogs table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing master DB SmsLogs table");
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
                    var connectionString = reader["ConnectionString"] != DBNull.Value
                        ? reader["ConnectionString"].ToString()
                        : null;

                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        result.Add(connectionString);
                    }
                }
            }
        }

        return result;
    }

    private void EnsureSmsLogsTable(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            var cmdCheck = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'SmsLogs'", connection);

            int tableCount = (int)cmdCheck.ExecuteScalar();

            if (tableCount == 0)
            {
                var cmdCreate = new SqlCommand(@"
                    CREATE TABLE [dbo].[SmsLogs] (
                        [ID] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [FromPhoneNumber] NVARCHAR(20) NOT NULL,
                        [ToPhoneNumber] NVARCHAR(20) NOT NULL,
                        [Message] NVARCHAR(MAX) NOT NULL,
                        [DateCreated] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
                        [TenantID] BIGINT NULL,
                        [TenantName] NVARCHAR(255) NULL
                    )", connection);

                cmdCreate.ExecuteNonQuery();
                _logger.LogInformation("SmsLogs table created.");
            }
            else
            {
                EnsureSmsLogsColumns(connection);
                EnsureTenantIdColumnType(connection);
            }
        }
    }

    private void EnsureSmsLogsColumns(SqlConnection connection)
    {
        var expectedColumns = new Dictionary<string, string>
        {
            ["FromPhoneNumber"] = "NVARCHAR(20) NOT NULL",
            ["ToPhoneNumber"] = "NVARCHAR(20) NOT NULL",
            ["Message"] = "NVARCHAR(MAX) NOT NULL",
            ["DateCreated"] = "DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()",
            ["TenantID"] = "BIGINT NULL",
            ["TenantName"] = "NVARCHAR(255) NULL"
        };

        foreach (var kvp in expectedColumns)
        {
            var columnName = kvp.Key;
            var columnType = kvp.Value;

            var cmdColumnCheck = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'SmsLogs' AND COLUMN_NAME = @ColumnName", connection);
            cmdColumnCheck.Parameters.AddWithValue("@ColumnName", columnName);

            int colExists = (int)cmdColumnCheck.ExecuteScalar();

            if (colExists == 0)
            {
                var alterCmd = new SqlCommand(
                    $"ALTER TABLE [dbo].[SmsLogs] ADD [{columnName}] {columnType}", connection);
                alterCmd.ExecuteNonQuery();

                _logger.LogInformation($"Column added to SmsLogs: {columnName} ({columnType})");
            }
        }
    }

    private void EnsureTenantIdColumnType(SqlConnection connection)
    {
        var checkColumn = new SqlCommand(@"
            SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'SmsLogs' AND COLUMN_NAME = 'TenantID'", connection);

        var dataType = checkColumn.ExecuteScalar()?.ToString();

        if (dataType != null && dataType.Equals("int", StringComparison.OrdinalIgnoreCase))
        {
            var alterColumn = new SqlCommand(@"
                ALTER TABLE [dbo].[SmsLogs]
                ALTER COLUMN [TenantID] BIGINT NULL", connection);

            alterColumn.ExecuteNonQuery();
            _logger.LogInformation("TenantID column type changed to BIGINT in SmsLogs.");
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureSmsLogsTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(masterConnectionString))
            {
                throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");
            }

            var enhancer = new TenantSchemaEnhancerEnsureSmsLogsTable(masterConnectionString, logger);

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
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureSmsLogsTable>>();
            fallbackLogger?.LogError(ex, "Error while processing SmsLogs table.");
        }
    }
}
