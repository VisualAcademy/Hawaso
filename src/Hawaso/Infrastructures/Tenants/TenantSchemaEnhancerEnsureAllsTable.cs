using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace Azunt.Infrastructures.Tenants;

public class TenantSchemaEnhancerEnsureAllsTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureAllsTable> _logger;

    public TenantSchemaEnhancerEnsureAllsTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureAllsTable> logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(masterConnectionString);

        _masterConnectionString = masterConnectionString;
        _logger = logger;
    }

    public void EnhanceTenantDatabases()
    {
        var tenantConnectionStrings = GetTenantConnectionStrings();

        foreach (var connectionString in tenantConnectionStrings)
        {
            try
            {
                EnsureAllsTable(connectionString);

                _logger.LogInformation(
                    "Alls table processed (tenant DB): {ConnectionString}",
                    connectionString);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[{ConnectionString}] Error processing tenant DB",
                    connectionString);
            }
        }
    }

    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureAllsTable(_masterConnectionString);

            _logger.LogInformation(
                "Alls table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing master DB");
        }
    }

    private List<string> GetTenantConnectionStrings()
    {
        var result = new List<string>();

        using var connection = new SqlConnection(_masterConnectionString);
        connection.Open();

        using var command = new SqlCommand(
            "SELECT ConnectionString FROM dbo.Tenants",
            connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (reader.IsDBNull(0))
            {
                _logger.LogWarning(
                    "A tenant with a null ConnectionString was skipped.");

                continue;
            }

            var connectionString = reader.GetString(0);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogWarning(
                    "A tenant with an empty ConnectionString was skipped.");

                continue;
            }

            result.Add(connectionString);
        }

        return result;
    }

    private void EnsureAllsTable(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Check if 'Alls' table exists
        using var cmdCheck = new SqlCommand(
            """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = 'dbo'
              AND TABLE_NAME = 'Alls'
            """,
            connection);

        var tableCount = Convert.ToInt32(cmdCheck.ExecuteScalar());

        if (tableCount == 0)
        {
            // Create 'Alls' table if it doesn't exist
            using var cmdCreate = new SqlCommand(
                """
                CREATE TABLE [dbo].[Alls] (
                    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [ParentId] BIGINT NULL,
                    [ParentKey] NVARCHAR(255) NULL,
                    [CreatedBy] NVARCHAR(255) NULL,
                    [Created] DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET(),
                    [ModifiedBy] NVARCHAR(255) NULL,
                    [Modified] DATETIMEOFFSET NULL,
                    [Name] NVARCHAR(255) NULL,
                    [PostDate] DATETIME NULL DEFAULT GETDATE(),
                    [PostIp] NVARCHAR(20) NULL,
                    [Title] NVARCHAR(512) NULL,
                    [Content] NTEXT NULL,
                    [Category] NVARCHAR(255) NULL DEFAULT 'Free',
                    [Email] NVARCHAR(255) NULL,
                    [Password] NVARCHAR(255) NULL,
                    [ReadCount] INT NULL DEFAULT 0,
                    [Encoding] NVARCHAR(20) NULL DEFAULT 'HTML',
                    [Homepage] NVARCHAR(100) NULL,
                    [ModifyDate] DATETIME NULL,
                    [ModifyIp] NVARCHAR(15) NULL,
                    [CommentCount] INT NULL DEFAULT 0,
                    [IsPinned] BIT NULL DEFAULT 0,
                    [FileName] NVARCHAR(255) NULL,
                    [FileSize] INT NULL DEFAULT 0,
                    [DownCount] INT NULL DEFAULT 0,
                    [Ref] INT NULL DEFAULT 0,
                    [Step] INT NULL DEFAULT 0,
                    [RefOrder] INT NULL DEFAULT 0,
                    [AnswerNum] INT NULL DEFAULT 0,
                    [ParentNum] INT NULL DEFAULT 0,
                    [Status] NVARCHAR(255) NULL,
                    [TenantId] INT NULL DEFAULT 0,
                    [TenantName] NVARCHAR(255) NULL,
                    [AppId] INT NULL DEFAULT 0,
                    [AppName] NVARCHAR(255) NULL,
                    [ModuleId] INT NULL DEFAULT 0,
                    [ModuleName] NVARCHAR(255) NULL,
                    [IsLocked] BIT NULL DEFAULT 0,
                    [Vote] INT NULL DEFAULT 0,
                    [Weather] TINYINT NULL DEFAULT 0,
                    [ReplyEmail] BIT NULL DEFAULT 0,
                    [Published] BIT NULL DEFAULT 0,
                    [BoardType] NVARCHAR(100) NULL,
                    [BoardName] NVARCHAR(255) NULL,
                    [NickName] NVARCHAR(255) NULL,
                    [IconName] NVARCHAR(100) NULL,
                    [Price] DECIMAL(18,2) NULL DEFAULT 0.00,
                    [Community] NVARCHAR(255) NULL,
                    [StartDate] DATETIMEOFFSET(7) NULL,
                    [EndDate] DATETIMEOFFSET(7) NULL,
                    [Video] NVARCHAR(1024) NULL,
                    [SecurityLevel] NVARCHAR(10) NULL,
                    [AvailableCustomerLevel] NVARCHAR(10) NULL,
                    [Num] INT NULL DEFAULT 0,
                    [UID] INT NULL DEFAULT 0,
                    [UserId] NVARCHAR(255) NULL,
                    [UserName] NVARCHAR(255) NULL,
                    [DivisionId] INT NULL DEFAULT 0,
                    [CategoryId] INT NULL DEFAULT 0,
                    [BoardId] INT NULL DEFAULT 0,
                    [ApplicationId] INT NULL DEFAULT 0,
                    [IsDeleted] BIT NULL DEFAULT 0,
                    [DeletedBy] NVARCHAR(255) NULL,
                    [Deleted] DATETIMEOFFSET NULL,
                    [ApprovalStatus] NVARCHAR(50) NULL,
                    [ApprovalBy] NVARCHAR(255) NULL,
                    [ApprovalDate] DATETIMEOFFSET NULL,
                    [UserAgent] NVARCHAR(512) NULL,
                    [Referer] NVARCHAR(512) NULL,
                    [SessionId] NVARCHAR(255) NULL,
                    [DisplayOrder] INT NULL DEFAULT 0,
                    [ViewRoles] NVARCHAR(255) NULL,
                    [Tags] NVARCHAR(255) NULL,
                    [LikeCount] INT NULL DEFAULT 0,
                    [DislikeCount] INT NULL DEFAULT 0,
                    [Rating] DECIMAL(3,2) NULL DEFAULT 0.0,
                    [Culture] NVARCHAR(10) NULL
                )
                """,
                connection);

            cmdCreate.ExecuteNonQuery();

            _logger.LogInformation(
                "Alls table created.");
        }

        // Check and add missing columns (excluding Id)
        var expectedColumns = new Dictionary<string, string>
        {
            ["ParentId"] = "BIGINT NULL",
            ["ParentKey"] = "NVARCHAR(255) NULL",
            ["CreatedBy"] = "NVARCHAR(255) NULL",
            ["Created"] = "DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET()",
            ["ModifiedBy"] = "NVARCHAR(255) NULL",
            ["Modified"] = "DATETIMEOFFSET NULL",
            ["Name"] = "NVARCHAR(255) NULL",
            ["PostDate"] = "DATETIME NULL DEFAULT GETDATE()",
            ["PostIp"] = "NVARCHAR(20) NULL",
            ["Title"] = "NVARCHAR(512) NULL",
            ["Content"] = "NTEXT NULL",
            ["Category"] = "NVARCHAR(255) NULL DEFAULT 'Free'",
            ["Email"] = "NVARCHAR(255) NULL",
            ["Password"] = "NVARCHAR(255) NULL",
            ["ReadCount"] = "INT NULL DEFAULT 0",
            ["Encoding"] = "NVARCHAR(20) NULL DEFAULT 'HTML'",
            ["Homepage"] = "NVARCHAR(100) NULL",
            ["ModifyDate"] = "DATETIME NULL",
            ["ModifyIp"] = "NVARCHAR(15) NULL",
            ["CommentCount"] = "INT NULL DEFAULT 0",
            ["IsPinned"] = "BIT NULL DEFAULT 0",
            ["FileName"] = "NVARCHAR(255) NULL",
            ["FileSize"] = "INT NULL DEFAULT 0",
            ["DownCount"] = "INT NULL DEFAULT 0",
            ["Ref"] = "INT NULL DEFAULT 0",
            ["Step"] = "INT NULL DEFAULT 0",
            ["RefOrder"] = "INT NULL DEFAULT 0",
            ["AnswerNum"] = "INT NULL DEFAULT 0",
            ["ParentNum"] = "INT NULL DEFAULT 0",
            ["Status"] = "NVARCHAR(255) NULL",
            ["TenantId"] = "INT NULL DEFAULT 0",
            ["TenantName"] = "NVARCHAR(255) NULL",
            ["AppId"] = "INT NULL DEFAULT 0",
            ["AppName"] = "NVARCHAR(255) NULL",
            ["ModuleId"] = "INT NULL DEFAULT 0",
            ["ModuleName"] = "NVARCHAR(255) NULL",
            ["IsLocked"] = "BIT NULL DEFAULT 0",
            ["Vote"] = "INT NULL DEFAULT 0",
            ["Weather"] = "TINYINT NULL DEFAULT 0",
            ["ReplyEmail"] = "BIT NULL DEFAULT 0",
            ["Published"] = "BIT NULL DEFAULT 0",
            ["BoardType"] = "NVARCHAR(100) NULL",
            ["BoardName"] = "NVARCHAR(255) NULL",
            ["NickName"] = "NVARCHAR(255) NULL",
            ["IconName"] = "NVARCHAR(100) NULL",
            ["Price"] = "DECIMAL(18,2) NULL DEFAULT 0.00",
            ["Community"] = "NVARCHAR(255) NULL",
            ["StartDate"] = "DATETIMEOFFSET(7) NULL",
            ["EndDate"] = "DATETIMEOFFSET(7) NULL",
            ["Video"] = "NVARCHAR(1024) NULL",
            ["SecurityLevel"] = "NVARCHAR(10) NULL",
            ["AvailableCustomerLevel"] = "NVARCHAR(10) NULL",
            ["Num"] = "INT NULL DEFAULT 0",
            ["UID"] = "INT NULL DEFAULT 0",
            ["UserId"] = "NVARCHAR(255) NULL",
            ["UserName"] = "NVARCHAR(255) NULL",
            ["DivisionId"] = "INT NULL DEFAULT 0",
            ["CategoryId"] = "INT NULL DEFAULT 0",
            ["BoardId"] = "INT NULL DEFAULT 0",
            ["ApplicationId"] = "INT NULL DEFAULT 0",
            ["IsDeleted"] = "BIT NULL DEFAULT 0",
            ["DeletedBy"] = "NVARCHAR(255) NULL",
            ["Deleted"] = "DATETIMEOFFSET NULL",
            ["ApprovalStatus"] = "NVARCHAR(50) NULL",
            ["ApprovalBy"] = "NVARCHAR(255) NULL",
            ["ApprovalDate"] = "DATETIMEOFFSET NULL",
            ["UserAgent"] = "NVARCHAR(512) NULL",
            ["Referer"] = "NVARCHAR(512) NULL",
            ["SessionId"] = "NVARCHAR(255) NULL",
            ["DisplayOrder"] = "INT NULL DEFAULT 0",
            ["ViewRoles"] = "NVARCHAR(255) NULL",
            ["Tags"] = "NVARCHAR(255) NULL",
            ["LikeCount"] = "INT NULL DEFAULT 0",
            ["DislikeCount"] = "INT NULL DEFAULT 0",
            ["Rating"] = "DECIMAL(3,2) NULL DEFAULT 0.0",
            ["Culture"] = "NVARCHAR(10) NULL"
        };

        foreach (var (columnName, columnDefinition) in expectedColumns)
        {
            using var cmdColCheck = new SqlCommand(
                """
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = 'dbo'
                  AND TABLE_NAME = 'Alls'
                  AND COLUMN_NAME = @ColumnName
                """,
                connection);

            cmdColCheck.Parameters.AddWithValue(
                "@ColumnName",
                columnName);

            var columnExists = Convert.ToInt32(
                cmdColCheck.ExecuteScalar());

            if (columnExists != 0)
            {
                continue;
            }

            using var cmdAlter = new SqlCommand(
                $"""
                ALTER TABLE [dbo].[Alls]
                ADD [{columnName}] {columnDefinition}
                """,
                connection);

            cmdAlter.ExecuteNonQuery();

            _logger.LogInformation(
                "Column added to Alls: {ColumnName} ({ColumnDefinition})",
                columnName,
                columnDefinition);
        }
    }

    public static void Run(
        IServiceProvider services,
        bool forMaster)
    {
        try
        {
            var logger =
                services.GetRequiredService<
                    ILogger<TenantSchemaEnhancerEnsureAllsTable>>();

            var configuration =
                services.GetRequiredService<IConfiguration>();

            var masterConnectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not configured.");

            var enhancer =
                new TenantSchemaEnhancerEnsureAllsTable(
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
            var fallbackLogger =
                services.GetService<
                    ILogger<TenantSchemaEnhancerEnsureAllsTable>>();

            fallbackLogger?.LogError(
                ex,
                "Error while processing Alls table.");
        }
    }
}