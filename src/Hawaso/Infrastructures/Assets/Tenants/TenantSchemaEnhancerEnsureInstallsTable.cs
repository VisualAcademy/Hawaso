using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Web.Infrastructures.Assets.Tenants
{
    public class TenantSchemaEnhancerEnsureInstallsTable
    {
        private readonly string _masterConnectionString;
        private readonly ILogger<TenantSchemaEnhancerEnsureInstallsTable> _logger;

        public TenantSchemaEnhancerEnsureInstallsTable(
            string masterConnectionString,
            ILogger<TenantSchemaEnhancerEnsureInstallsTable> logger)
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
                    EnsureInstallsTable(connStr);
                    _logger.LogInformation($"Installs table processed (tenant DB): {connStr}");
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
                EnsureInstallsTable(_masterConnectionString);
                _logger.LogInformation("Installs table processed (master DB)");
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

        private void EnsureInstallsTable(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'Installs'", connection);

                int tableCount = (int)cmdCheck.ExecuteScalar();

                if (tableCount == 0)
                {
                    var cmdCreate = new SqlCommand(@"
                        CREATE TABLE [dbo].[Installs] (
                            [Id] INT NOT NULL PRIMARY KEY Identity(1, 1),
                            MachineId INT NOT NULL,
                            MediaId INT NOT NULL,
                            [CreatedBy] NVARCHAR(255) NULL,
                            [Created] DATETIME DEFAULT(GETDATE()) NULL,
                            [ModifiedBy] NVARCHAR(255) NULL,
                            [Modified] DATETIME NULL
                        )", connection);

                    cmdCreate.ExecuteNonQuery();
                    _logger.LogInformation("Installs table created.");
                }
            }
        }

        public static void Run(IServiceProvider services, bool forMaster)
        {
            try
            {
                var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureInstallsTable>>();
                var config = services.GetRequiredService<IConfiguration>();
                var masterConnectionString = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(masterConnectionString))
                {
                    throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");
                }

                var enhancer = new TenantSchemaEnhancerEnsureInstallsTable(masterConnectionString, logger);

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
                var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureInstallsTable>>();
                fallbackLogger?.LogError(ex, "Error while processing Installs table.");
            }
        }
    }
}
