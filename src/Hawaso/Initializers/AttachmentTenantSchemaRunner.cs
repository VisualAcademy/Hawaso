using Azunt.AttachmentManagement;

namespace Azunt.Initializers;

/// <summary>
/// 마스터 데이터베이스의 dbo.Tenants 테이블에서 연결 문자열을 조회한 후
/// 각 테넌트 데이터베이스의 Attachments 테이블을 생성하거나 확장합니다.
/// </summary>
public static class AttachmentTenantSchemaRunner
{
    public static async Task RunAsync(
        IServiceProvider services,
        string masterConnectionString,
        bool ensureIndexes = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(masterConnectionString))
        {
            throw new ArgumentException(
                "Master connection string is required.",
                nameof(masterConnectionString));
        }

        await using var scope = services.CreateAsyncScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<AttachmentsTableBuilder>>();

        var tableBuilder = scope.ServiceProvider
            .GetRequiredService<AttachmentsTableBuilder>();

        var tenantConnectionStrings =
            await GetTenantConnectionStringsAsync(
                masterConnectionString,
                cancellationToken);

        logger.LogInformation(
            "Starting Attachments schema initialization for {TenantCount} tenant databases.",
            tenantConnectionStrings.Count);

        var succeeded = 0;
        var failed = 0;

        for (var index = 0; index < tenantConnectionStrings.Count; index++)
        {
            var tenantConnectionString =
                tenantConnectionStrings[index];

            var tenantNumber = index + 1;

            try
            {
                await tableBuilder.EnsureAsync(
                    connectionString: tenantConnectionString,
                    ensureIndexes: ensureIndexes,
                    cancellationToken: cancellationToken);

                succeeded++;

                logger.LogInformation(
                    "Attachments schema initialization completed for tenant database #{TenantNumber}.",
                    tenantNumber);
            }
            catch (Exception exception)
            {
                failed++;

                logger.LogError(
                    exception,
                    "Attachments schema initialization failed for tenant database #{TenantNumber}.",
                    tenantNumber);
            }
        }

        logger.LogInformation(
            "Attachments schema initialization finished. Succeeded: {SucceededCount}, Failed: {FailedCount}.",
            succeeded,
            failed);
    }

    private static async Task<List<string>>
        GetTenantConnectionStringsAsync(
            string masterConnectionString,
            CancellationToken cancellationToken)
    {
        var results = new List<string>();

        await using var connection =
            new SqlConnection(masterConnectionString);

        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT [ConnectionString]
            FROM [dbo].[Tenants]
            WHERE [ConnectionString] IS NOT NULL
              AND LTRIM(RTRIM([ConnectionString])) <> '';
            """;

        await using var command =
            new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var connectionString =
                reader["ConnectionString"]?.ToString();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                results.Add(connectionString);
            }
        }

        return results;
    }
}