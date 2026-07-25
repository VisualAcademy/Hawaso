using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Infrastructures.Tenants;

public class TenantSchemaEnhancerEnsureContactTypesTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureContactTypesTable> _logger;

    public TenantSchemaEnhancerEnsureContactTypesTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureContactTypesTable> logger)
    {
        if (string.IsNullOrWhiteSpace(masterConnectionString))
        {
            throw new ArgumentException(
                "Master database connection string cannot be null or empty.",
                nameof(masterConnectionString));
        }

        _masterConnectionString = masterConnectionString;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void EnhanceTenantDatabases()
    {
        var tenantConnectionStrings = GetTenantConnectionStrings();

        foreach (var connectionString in tenantConnectionStrings)
        {
            try
            {
                EnsureContactTypesTable(connectionString);

                _logger.LogInformation(
                    "ContactTypes table processed for tenant database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing a tenant database.");
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureContactTypesTable(_masterConnectionString);

            _logger.LogInformation(
                "ContactTypes table processed for master database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing master database.");
        }
    }

    private List<string> GetTenantConnectionStrings()
    {
        var result = new List<string>();

        using var connection = new SqlConnection(_masterConnectionString);
        connection.Open();

        const string sql = """
            SELECT ConnectionString
            FROM dbo.Tenants
            """;

        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (reader["ConnectionString"] is string connectionString &&
                !string.IsNullOrWhiteSpace(connectionString))
            {
                result.Add(connectionString);
            }
            else
            {
                _logger.LogWarning(
                    "A tenant record has a null or empty connection string and was skipped.");
            }
        }

        return result;
    }

    private void EnsureContactTypesTable(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "Tenant database connection string cannot be null or empty.",
                nameof(connectionString));
        }

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        const string tableCheckSql = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'ContactTypes'
            """;

        using (var tableCheckCommand =
               new SqlCommand(tableCheckSql, connection))
        {
            var tableCount = Convert.ToInt32(
                tableCheckCommand.ExecuteScalar());

            if (tableCount == 0)
            {
                const string createTableSql = """
                    CREATE TABLE [dbo].[ContactTypes]
                    (
                        [ID] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Active] BIT NOT NULL DEFAULT ((1)),
                        [CreatedAt] DATETIMEOFFSET(7) NOT NULL,
                        [CreatedBy] NVARCHAR(70) NULL,
                        [Label] NVARCHAR(255) NULL
                    )
                    """;

                using var createTableCommand =
                    new SqlCommand(createTableSql, connection);

                createTableCommand.ExecuteNonQuery();

                _logger.LogInformation(
                    "ContactTypes table created.");
            }
        }

        const string columnCheckSql = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'ContactTypes'
              AND COLUMN_NAME = 'Description'
            """;

        using (var columnCheckCommand =
               new SqlCommand(columnCheckSql, connection))
        {
            var columnCount = Convert.ToInt32(
                columnCheckCommand.ExecuteScalar());

            if (columnCount == 0)
            {
                const string addColumnSql = """
                    ALTER TABLE [dbo].[ContactTypes]
                    ADD [Description] NVARCHAR(MAX) NULL
                    """;

                using var addColumnCommand =
                    new SqlCommand(addColumnSql, connection);

                addColumnCommand.ExecuteNonQuery();

                _logger.LogInformation(
                    "Description column added to ContactTypes.");
            }
        }

        EnsureDefaultContactTypes(connection);
    }

    private void EnsureDefaultContactTypes(SqlConnection connection)
    {
        const string rowCountSql = """
            SELECT COUNT(*)
            FROM [dbo].[ContactTypes]
            """;

        using var rowCountCommand =
            new SqlCommand(rowCountSql, connection);

        var rowCount = Convert.ToInt32(
            rowCountCommand.ExecuteScalar());

        if (rowCount > 0)
        {
            _logger.LogInformation(
                "ContactTypes table already contains data. " +
                "Skipping default insert.");

            return;
        }

        var defaultTypes =
            new List<(string Label, string Description)>
            {
                ("Primary", "Main point of contact."),
                ("Secondary", "Alternative point of contact.")
            };

        foreach (var (label, description) in defaultTypes)
        {
            const string insertSql = """
                INSERT INTO [dbo].[ContactTypes]
                (
                    [Active],
                    [CreatedAt],
                    [CreatedBy],
                    [Label],
                    [Description]
                )
                VALUES
                (
                    1,
                    SYSDATETIMEOFFSET(),
                    'System',
                    @Label,
                    @Description
                )
                """;

            using var insertCommand =
                new SqlCommand(insertSql, connection);

            insertCommand.Parameters.AddWithValue(
                "@Label",
                label);

            insertCommand.Parameters.AddWithValue(
                "@Description",
                description);

            insertCommand.ExecuteNonQuery();

            _logger.LogInformation(
                "Default ContactType inserted: {Label}",
                label);
        }
    }

    public static void Run(
        IServiceProvider services,
        bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<
                ILogger<TenantSchemaEnhancerEnsureContactTypesTable>>();

            var configuration =
                services.GetRequiredService<IConfiguration>();

            var masterConnectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "The 'DefaultConnection' connection string is not configured.");

            if (string.IsNullOrWhiteSpace(masterConnectionString))
            {
                throw new InvalidOperationException(
                    "The 'DefaultConnection' connection string is empty.");
            }

            var enhancer =
                new TenantSchemaEnhancerEnsureContactTypesTable(
                    masterConnectionString,
                    logger);

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
            var fallbackLogger = services.GetService<
                ILogger<TenantSchemaEnhancerEnsureContactTypesTable>>();

            fallbackLogger?.LogError(
                ex,
                "Error while processing ContactTypes table.");
        }
    }
}