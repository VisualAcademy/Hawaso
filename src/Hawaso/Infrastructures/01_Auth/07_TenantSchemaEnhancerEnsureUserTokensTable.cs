using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Web.Infrastructures.Auth;

/// <summary>
/// AspNetUserTokens 테이블을 생성하고 외래 키 및 인덱스를 보장하는 클래스입니다.
/// 이 테이블이 없으면 사용자 인증 토큰 저장 기능이 작동하지 않습니다.
/// </summary>
public class TenantSchemaEnhancerEnsureUserTokensTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureUserTokensTable> _logger;

    public TenantSchemaEnhancerEnsureUserTokensTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureUserTokensTable> logger)
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
                EnsureUserTokensTable(connStr);
                _logger.LogInformation($"AspNetUserTokens table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing AspNetUserTokens table");
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureUserTokensTable(_masterConnectionString);
            _logger.LogInformation("AspNetUserTokens table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AspNetUserTokens table (master DB)");
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

    private void EnsureUserTokensTable(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var cmdCheckTable = new SqlCommand(@"
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'AspNetUserTokens'", connection);

        int tableExists = (int)cmdCheckTable.ExecuteScalar();

        if (tableExists == 0)
        {
            var createCmd = new SqlCommand(@"
                CREATE TABLE [dbo].[AspNetUserTokens] (
                    [UserId]        NVARCHAR(450) NOT NULL,
                    [LoginProvider] NVARCHAR(450) NOT NULL,
                    [Name]          NVARCHAR(450) NOT NULL,
                    [Value]         NVARCHAR(MAX) NULL,
                    CONSTRAINT [PK_AspNetUserTokens] 
                        PRIMARY KEY CLUSTERED ([UserId], [LoginProvider], [Name]),
                    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] 
                        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
                );
            ", connection);

            createCmd.ExecuteNonQuery();
            _logger.LogInformation("AspNetUserTokens table created.");
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureUserTokensTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection")!;

            var enhancer = new TenantSchemaEnhancerEnsureUserTokensTable(masterConnectionString, logger);

            if (forMaster)
                enhancer.EnhanceMasterDatabase();
            else
                enhancer.EnhanceTenantDatabases();
        }
        catch (Exception ex)
        {
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureUserTokensTable>>();
            fallbackLogger?.LogError(ex, "Error while processing AspNetUserTokens table.");
        }
    }
}
