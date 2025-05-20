using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Web.Infrastructures.Auth;

/// <summary>
/// Entity Framework의 __EFMigrationsHistory 테이블을 보장하는 클래스입니다.
/// 이 테이블은 마이그레이션 내역을 저장하며 EF Core에서 자동 관리됩니다.
/// </summary>
public class TenantSchemaEnhancerEnsureMigrationsHistoryTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureMigrationsHistoryTable> _logger;

    public TenantSchemaEnhancerEnsureMigrationsHistoryTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureMigrationsHistoryTable> logger)
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
                EnsureMigrationsHistoryTable(connStr);
                _logger.LogInformation($"__EFMigrationsHistory table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing __EFMigrationsHistory table");
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureMigrationsHistoryTable(_masterConnectionString);
            _logger.LogInformation("__EFMigrationsHistory table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing __EFMigrationsHistory table (master DB)");
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

    private void EnsureMigrationsHistoryTable(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var cmdCheckTable = new SqlCommand(@"
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = '__EFMigrationsHistory'", connection);

        int tableExists = (int)cmdCheckTable.ExecuteScalar();

        if (tableExists == 0)
        {
            var createCmd = new SqlCommand(@"
                CREATE TABLE [dbo].[__EFMigrationsHistory] (
                    [MigrationId] NVARCHAR(150) NOT NULL,
                    [ProductVersion] NVARCHAR(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
                );
            ", connection);

            createCmd.ExecuteNonQuery();
            _logger.LogInformation("__EFMigrationsHistory table created.");
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureMigrationsHistoryTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection")!;

            var enhancer = new TenantSchemaEnhancerEnsureMigrationsHistoryTable(masterConnectionString, logger);

            if (forMaster)
                enhancer.EnhanceMasterDatabase();
            else
                enhancer.EnhanceTenantDatabases();
        }
        catch (Exception ex)
        {
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureMigrationsHistoryTable>>();
            fallbackLogger?.LogError(ex, "Error while processing __EFMigrationsHistory table.");
        }
    }
}
