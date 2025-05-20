using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Web.Infrastructures.Auth;

/// <summary>
/// AspNetUserLogins 테이블을 생성하고 외래 키 및 인덱스를 보장하는 클래스입니다.
/// 이 테이블이 없으면 외부 로그인 기능이 작동하지 않습니다.
/// </summary>
public class TenantSchemaEnhancerEnsureUserLoginsTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureUserLoginsTable> _logger;

    public TenantSchemaEnhancerEnsureUserLoginsTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureUserLoginsTable> logger)
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
                EnsureUserLoginsTable(connStr);
                _logger.LogInformation($"AspNetUserLogins table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing AspNetUserLogins table");
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureUserLoginsTable(_masterConnectionString);
            _logger.LogInformation("AspNetUserLogins table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AspNetUserLogins table (master DB)");
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

    private void EnsureUserLoginsTable(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var cmdCheckTable = new SqlCommand(@"
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'AspNetUserLogins'", connection);

        int tableExists = (int)cmdCheckTable.ExecuteScalar();

        if (tableExists == 0)
        {
            var createCmd = new SqlCommand(@"
                CREATE TABLE [dbo].[AspNetUserLogins] (
                    [LoginProvider] NVARCHAR(450) NOT NULL,
                    [ProviderKey] NVARCHAR(450) NOT NULL,
                    [ProviderDisplayName] NVARCHAR(MAX) NULL,
                    [UserId] NVARCHAR(450) NOT NULL,
                    CONSTRAINT [PK_AspNetUserLogins] 
                        PRIMARY KEY CLUSTERED ([LoginProvider], [ProviderKey]),
                    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] 
                        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
                );

                CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId]
                ON [dbo].[AspNetUserLogins]([UserId]);
            ", connection);

            createCmd.ExecuteNonQuery();
            _logger.LogInformation("AspNetUserLogins table created.");
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureUserLoginsTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection")!;

            var enhancer = new TenantSchemaEnhancerEnsureUserLoginsTable(masterConnectionString, logger);

            if (forMaster)
                enhancer.EnhanceMasterDatabase();
            else
                enhancer.EnhanceTenantDatabases();
        }
        catch (Exception ex)
        {
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureUserLoginsTable>>();
            fallbackLogger?.LogError(ex, "Error while processing AspNetUserLogins table.");
        }
    }
}
