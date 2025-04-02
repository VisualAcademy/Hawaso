using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using Microsoft.Data.SqlClient;

namespace Azunt.Infrastructures.Tenants;

public class TenantSchemaEnhancerEnsureContactTypesTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureContactTypesTable> _logger;

    public TenantSchemaEnhancerEnsureContactTypesTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureContactTypesTable> logger)
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
                EnsureContactTypesTable(connStr);
                _logger.LogInformation($"ContactTypes table processed (tenant DB): {connStr}");
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
            EnsureContactTypesTable(_masterConnectionString);
            _logger.LogInformation("ContactTypes table processed (master DB)");
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
                    result.Add(reader["ConnectionString"].ToString());
                }
            }
        }

        return result;
    }

    private void EnsureContactTypesTable(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            var cmdCheck = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'ContactTypes'", connection);

            int tableCount = (int)cmdCheck.ExecuteScalar();

            if (tableCount == 0)
            {
                var cmdCreate = new SqlCommand(@"
                    CREATE TABLE [dbo].[ContactTypes] (
                        [ID] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Active] BIT NOT NULL DEFAULT ((1)),
                        [CreatedAt] DATETIMEOFFSET(7) NOT NULL,
                        [CreatedBy] NVARCHAR(70) NULL,
                        [Label] NVARCHAR(255) NULL
                    )", connection);
                cmdCreate.ExecuteNonQuery();

                _logger.LogInformation("ContactTypes table created.");
            }

            // Check and add Description column if missing
            var cmdColCheck = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'ContactTypes' AND COLUMN_NAME = 'Description'", connection);

            int colExists = (int)cmdColCheck.ExecuteScalar();

            if (colExists == 0)
            {
                var cmdAddCol = new SqlCommand(@"
                    ALTER TABLE [dbo].[ContactTypes] 
                    ADD [Description] NVARCHAR(MAX) NULL", connection);
                cmdAddCol.ExecuteNonQuery();

                _logger.LogInformation("Description column added to ContactTypes.");
            }

            EnsureDefaultContactTypes(connection);
        }
    }

    private void EnsureDefaultContactTypes(SqlConnection connection)
    {
        var cmdRowCount = new SqlCommand("SELECT COUNT(*) FROM [dbo].[ContactTypes]", connection);
        int rowCount = (int)cmdRowCount.ExecuteScalar();

        if (rowCount > 0)
        {
            _logger.LogInformation("ContactTypes table already contains data. Skipping default insert.");
            return;
        }

        var defaultTypes = new List<(string Label, string Description)>
        {
            ("Primary", "Main point of contact."),
            ("Secondary", "Alternative point of contact.")
        };

        foreach (var (label, description) in defaultTypes)
        {
            var cmdInsert = new SqlCommand(@"
                INSERT INTO [dbo].[ContactTypes] 
                ([Active], [CreatedAt], [CreatedBy], [Label], [Description]) 
                VALUES (1, SYSDATETIMEOFFSET(), 'System', @Label, @Description)", connection);

            cmdInsert.Parameters.AddWithValue("@Label", label);
            cmdInsert.Parameters.AddWithValue("@Description", description);
            cmdInsert.ExecuteNonQuery();

            _logger.LogInformation($"Default ContactType inserted: {label}");
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureContactTypesTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection");

            var enhancer = new TenantSchemaEnhancerEnsureContactTypesTable(masterConnectionString, logger);

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
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureContactTypesTable>>();
            fallbackLogger?.LogError(ex, "Error while processing ContactTypes table.");
        }
    }
}
