using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Azunt.Infrastructures.Employees;

/// <summary>
/// EligibilityTypes 테이블을 생성하고 기본 자격 상태 데이터를 삽입하는 클래스입니다.
/// </summary>
public class EmployeeSchemaEnhancerEnsureEligibilityTypesTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<EmployeeSchemaEnhancerEnsureEligibilityTypesTable> _logger;

    public EmployeeSchemaEnhancerEnsureEligibilityTypesTable(
        string masterConnectionString,
        ILogger<EmployeeSchemaEnhancerEnsureEligibilityTypesTable> logger)
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
                EnsureEligibilityTypesTable(connStr);
                _logger.LogInformation($"EligibilityTypes table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing EligibilityTypes table");
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureEligibilityTypesTable(_masterConnectionString);
            _logger.LogInformation("EligibilityTypes table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing EligibilityTypes table (master DB)");
        }
    }

    private List<string> GetTenantConnectionStrings()
    {
        var result = new List<string>();

        using var connection = new SqlConnection(_masterConnectionString);
        connection.Open();

        var cmd = new SqlCommand("SELECT ConnectionString FROM dbo.Tenants", connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var connStr = reader["ConnectionString"]?.ToString();
            if (!string.IsNullOrEmpty(connStr))
                result.Add(connStr);
        }

        return result;
    }

    private void EnsureEligibilityTypesTable(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var cmdCheckTable = new SqlCommand(@"
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'EligibilityTypes'", connection);
        var tableExists = (int)cmdCheckTable.ExecuteScalar();

        if (tableExists == 0)
        {
            var createCmd = new SqlCommand(@"
                CREATE TABLE [dbo].[EligibilityTypes](
                    [ID] [bigint] IDENTITY(1,1) NOT NULL,
                    [Label] [nvarchar](255) NULL,
                    [Value] [nvarchar](max) NULL,
                    CONSTRAINT [PK_EligibilityTypes] PRIMARY KEY CLUSTERED ([ID] ASC)
                );", connection);

            createCmd.ExecuteNonQuery();
            _logger.LogInformation("EligibilityTypes table created.");
        }

        EnsureDefaultEligibilityTypes(connection);
    }

    private void EnsureDefaultEligibilityTypes(SqlConnection connection)
    {
        var checkCmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[EligibilityTypes]", connection);
        int count = (int)checkCmd.ExecuteScalar();

        if (count == 0)
        {
            var types = new List<(string Label, string Value)>
            {
                ("Conditional", "Should be granted a conditional license"),
                ("Denial", "Should be denied a license"),
                ("Suspension", "Should be suspended"),
                ("Revocation", "Should be revoked"),
                ("Eligible", "should grant a license")
            };

            foreach (var (label, value) in types)
            {
                var insertCmd = new SqlCommand(@"
                    INSERT INTO [dbo].[EligibilityTypes] ([Label], [Value])
                    VALUES (@Label, @Value)", connection);

                insertCmd.Parameters.AddWithValue("@Label", label);
                insertCmd.Parameters.AddWithValue("@Value", value);
                insertCmd.ExecuteNonQuery();

                _logger.LogInformation($"Inserted default eligibility type: {label} - {value}");
            }
        }
    }

    /// <summary>
    /// Program.cs 또는 Startup.cs에서 호출되는 진입점입니다.
    /// </summary>
    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<EmployeeSchemaEnhancerEnsureEligibilityTypesTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection")!;

            var enhancer = new EmployeeSchemaEnhancerEnsureEligibilityTypesTable(masterConnectionString, logger);

            if (forMaster)
                enhancer.EnhanceMasterDatabase();
            else
                enhancer.EnhanceTenantDatabases();
        }
        catch (Exception ex)
        {
            var fallbackLogger = services.GetService<ILogger<EmployeeSchemaEnhancerEnsureEligibilityTypesTable>>();
            fallbackLogger?.LogError(ex, "Error while processing EligibilityTypes table");
        }
    }
}