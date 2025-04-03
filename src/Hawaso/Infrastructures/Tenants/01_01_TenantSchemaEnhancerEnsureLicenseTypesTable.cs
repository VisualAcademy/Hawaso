﻿namespace Azunt.Infrastructures.Tenants;

public class TenantSchemaEnhancerEnsureLicenseTypesTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureLicenseTypesTable> _logger;

    public TenantSchemaEnhancerEnsureLicenseTypesTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureLicenseTypesTable> logger)
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
                EnsureLicenseTypesTable(connStr);
                _logger.LogInformation($"LicenseTypes table processed (tenant DB): {connStr}");
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
            EnsureLicenseTypesTable(_masterConnectionString);
            _logger.LogInformation("LicenseTypes table processed (master DB)");
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

    private void EnsureLicenseTypesTable(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            var cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'LicenseTypes'", connection);

            int tableCount = (int)cmdCheck.ExecuteScalar();

            if (tableCount == 0)
            {
                var cmdCreate = new SqlCommand(@"
                        CREATE TABLE [dbo].[LicenseTypes] (
                            [ID]            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Active]        BIT NULL,
                            [CreatedAt]     DATETIMEOFFSET(7) NULL,
                            [CreatedBy]     NVARCHAR(70) NULL,
                            [Type]          NVARCHAR(450) NULL,
                            [Description]   NVARCHAR(MAX) NULL,
                            [ApplicantType] INT NULL,
                            [BgRequired]    BIT NULL
                        )", connection);
                cmdCreate.ExecuteNonQuery();

                _logger.LogInformation("LicenseTypes table created.");
            }
            else
            {
                var expectedColumns = new Dictionary<string, string>
                {
                    ["Active"] = "BIT NULL",
                    ["CreatedAt"] = "DATETIMEOFFSET(7) NULL",
                    ["CreatedBy"] = "NVARCHAR(70) NULL",
                    ["Type"] = "NVARCHAR(450) NULL",
                    ["Description"] = "NVARCHAR(MAX) NULL",
                    ["ApplicantType"] = "INT NULL",
                    ["BgRequired"] = "BIT NULL"
                };

                foreach (var kvp in expectedColumns)
                {
                    var columnName = kvp.Key;
                    var columnType = kvp.Value;

                    var cmdColumnCheck = new SqlCommand(@"
                            SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = 'LicenseTypes' AND COLUMN_NAME = @ColumnName", connection);
                    cmdColumnCheck.Parameters.AddWithValue("@ColumnName", columnName);

                    int colExists = (int)cmdColumnCheck.ExecuteScalar();

                    if (colExists == 0)
                    {
                        var alterCmd = new SqlCommand(
                            $"ALTER TABLE [dbo].[LicenseTypes] ADD [{columnName}] {columnType}", connection);
                        alterCmd.ExecuteNonQuery();

                        _logger.LogInformation($"Column added: {columnName} ({columnType})");
                    }
                }
            }

            // Insert default data only if the table is empty
            EnsureDefaultLicenseTypes(connection);
        }
    }

    private void EnsureDefaultLicenseTypes(SqlConnection connection)
    {
        // Only insert if table is completely empty
        var cmdRowCount = new SqlCommand("SELECT COUNT(*) FROM [dbo].[LicenseTypes]", connection);
        int rowCount = (int)cmdRowCount.ExecuteScalar();

        if (rowCount > 0)
        {
            _logger.LogInformation("LicenseTypes table already contains data. Skipping default insert.");
            return;
        }

        var defaultTypes = new List<(string Type, string Description)>
        {
            ("Exempt", "A license type that is not subject to standard licensing requirements."),
            ("Registration", "A license type for entities that only require registration."),
            ("Temporary", "A license type issued for temporary purposes."),
            ("Permanent", "A license type granted with full approval."),
            ("Provisional", "A license type conditionally approved or under review.")
        };

        foreach (var (type, description) in defaultTypes)
        {
            var cmdInsert = new SqlCommand(@"
                    INSERT INTO [dbo].[LicenseTypes]
                    ([Active], [CreatedAt], [CreatedBy], [Type], [Description], [ApplicantType], [BgRequired])
                    VALUES (1, SYSDATETIMEOFFSET(), 'System', @Type, @Description, 1, 0)", connection);

            cmdInsert.Parameters.AddWithValue("@Type", type);
            cmdInsert.Parameters.AddWithValue("@Description", description);

            cmdInsert.ExecuteNonQuery();

            _logger.LogInformation($"Default LicenseType inserted: {type}");
        }
    }

    /// <summary>
    /// Entry point to run from Program.cs or Startup.cs
    /// forMaster == true: only master DB
    /// forMaster == false: only tenant DBs
    /// </summary>
    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureLicenseTypesTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection");

            var enhancer = new TenantSchemaEnhancerEnsureLicenseTypesTable(masterConnectionString, logger);

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
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureLicenseTypesTable>>();
            fallbackLogger?.LogError(ex, "Error while processing LicenseTypes table.");
        }
    }
}
