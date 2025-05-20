using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.Web.Infrastructures.Auth;

/// <summary>
/// AspNetRoleClaims 테이블을 생성하고 외래 키 및 인덱스를 보장하는 클래스입니다.
/// 이 테이블이 없으면 역할 기반 클레임 정책이 작동하지 않습니다.
/// </summary>
public class TenantSchemaEnhancerEnsureRoleClaimsTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureRoleClaimsTable> _logger;

    public TenantSchemaEnhancerEnsureRoleClaimsTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureRoleClaimsTable> logger)
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
                EnsureRoleClaimsTable(connStr);
                _logger.LogInformation($"AspNetRoleClaims table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing AspNetRoleClaims table");
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureRoleClaimsTable(_masterConnectionString);
            _logger.LogInformation("AspNetRoleClaims table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AspNetRoleClaims table (master DB)");
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

    private void EnsureRoleClaimsTable(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var cmdCheckTable = new SqlCommand(@"
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'AspNetRoleClaims'", connection);

        int tableExists = (int)cmdCheckTable.ExecuteScalar();

        if (tableExists == 0)
        {
            var createCmd = new SqlCommand(@"
                CREATE TABLE [dbo].[AspNetRoleClaims] (
                    [Id] INT IDENTITY(1,1) NOT NULL,
                    [RoleId] NVARCHAR(450) NOT NULL,
                    [ClaimType] NVARCHAR(MAX) NULL,
                    [ClaimValue] NVARCHAR(MAX) NULL,
                    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
                    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] 
                        FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
                );

                CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId]
                ON [dbo].[AspNetRoleClaims]([RoleId] ASC);
            ", connection);

            createCmd.ExecuteNonQuery();
            _logger.LogInformation("AspNetRoleClaims table created.");
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureRoleClaimsTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection")!;

            var enhancer = new TenantSchemaEnhancerEnsureRoleClaimsTable(masterConnectionString, logger);

            if (forMaster)
                enhancer.EnhanceMasterDatabase();
            else
                enhancer.EnhanceTenantDatabases();
        }
        catch (Exception ex)
        {
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureRoleClaimsTable>>();
            fallbackLogger?.LogError(ex, "Error while processing AspNetRoleClaims table.");
        }
    }
}
