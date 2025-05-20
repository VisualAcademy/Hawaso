using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Web.Infrastructures.Auth;

/// <summary>
/// AspNetUserClaims 테이블을 생성하고 외래 키 및 인덱스를 보장하는 클래스입니다.
/// 이 테이블이 없으면 사용자 클레임 기능이 작동하지 않습니다.
/// </summary>
public class TenantSchemaEnhancerEnsureUserClaimsTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureUserClaimsTable> _logger;

    public TenantSchemaEnhancerEnsureUserClaimsTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureUserClaimsTable> logger)
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
                EnsureUserClaimsTable(connStr);
                _logger.LogInformation($"AspNetUserClaims table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing AspNetUserClaims table");
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureUserClaimsTable(_masterConnectionString);
            _logger.LogInformation("AspNetUserClaims table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AspNetUserClaims table (master DB)");
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

    private void EnsureUserClaimsTable(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var cmdCheckTable = new SqlCommand(@"
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'AspNetUserClaims'", connection);

        int tableExists = (int)cmdCheckTable.ExecuteScalar();

        if (tableExists == 0)
        {
            var createCmd = new SqlCommand(@"
                CREATE TABLE [dbo].[AspNetUserClaims] (
                    [Id] INT IDENTITY(1,1) NOT NULL,
                    [UserId] NVARCHAR(450) NOT NULL,
                    [ClaimType] NVARCHAR(MAX) NULL,
                    [ClaimValue] NVARCHAR(MAX) NULL,
                    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
                    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] 
                        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) 
                        ON DELETE CASCADE
                );

                CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId]
                ON [dbo].[AspNetUserClaims]([UserId] ASC);
            ", connection);

            createCmd.ExecuteNonQuery();
            _logger.LogInformation("AspNetUserClaims table created.");
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureUserClaimsTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection")!;

            var enhancer = new TenantSchemaEnhancerEnsureUserClaimsTable(masterConnectionString, logger);

            if (forMaster)
                enhancer.EnhanceMasterDatabase();
            else
                enhancer.EnhanceTenantDatabases();
        }
        catch (Exception ex)
        {
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureUserClaimsTable>>();
            fallbackLogger?.LogError(ex, "Error while processing AspNetUserClaims table.");
        }
    }
}
