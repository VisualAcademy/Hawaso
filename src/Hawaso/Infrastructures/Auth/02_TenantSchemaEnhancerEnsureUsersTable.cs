namespace Azunt.Web.Infrastructures.Auth;

/// <summary>
/// 테넌트 및 마스터 데이터베이스에 AspNetUsers 테이블을 생성 및 보강하고,
/// 필요한 컬럼들을 추가하는 클래스입니다.
/// </summary>
public class TenantSchemaEnhancerEnsureUsersTable
{
    private readonly string _masterConnectionString;
    private readonly ILogger<TenantSchemaEnhancerEnsureUsersTable> _logger;

    /// <summary>
    /// 생성자: 마스터 연결 문자열과 로거를 받아 초기화합니다.
    /// </summary>
    /// <param name="masterConnectionString">마스터 데이터베이스 연결 문자열</param>
    /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
    public TenantSchemaEnhancerEnsureUsersTable(
        string masterConnectionString,
        ILogger<TenantSchemaEnhancerEnsureUsersTable> logger)
    {
        _masterConnectionString = masterConnectionString;
        _logger = logger;
    }

    /// <summary>
    /// 모든 테넌트 데이터베이스에서 AspNetUsers 테이블이 존재하는지 확인하고,
    /// 존재하지 않으면 생성하며, 누락된 컬럼이 있다면 추가합니다.
    /// </summary>
    public void EnhanceTenantDatabases()
    {
        var tenantConnectionStrings = GetTenantConnectionStrings();

        foreach (var connStr in tenantConnectionStrings)
        {
            try
            {
                EnsureUsersTable(connStr);
                _logger.LogInformation($"AspNetUsers table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing tenant DB");
            }
        }
    }

    /// <summary>
    /// 마스터 데이터베이스에서 AspNetUsers 테이블을 보장하고 컬럼을 보강합니다.
    /// </summary>
    public void EnhanceMasterDatabase()
    {
        try
        {
            EnsureUsersTable(_masterConnectionString);
            _logger.LogInformation("AspNetUsers table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing master DB");
        }
    }

    /// <summary>
    /// 마스터 데이터베이스에서 모든 테넌트의 연결 문자열을 조회합니다.
    /// </summary>
    /// <returns>테넌트 연결 문자열 리스트</returns>
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
                    var connectionString = reader["ConnectionString"]?.ToString();
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        result.Add(connectionString);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 주어진 연결 문자열에 대해 AspNetUsers 테이블 및 필수 컬럼을 보장합니다.
    /// </summary>
    /// <param name="connectionString">데이터베이스 연결 문자열</param>
    private void EnsureUsersTable(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // AspNetUsers 테이블 존재 여부 확인
            var cmdCheckTable = new SqlCommand(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'AspNetUsers'", connection);

            int tableExists = (int)cmdCheckTable.ExecuteScalar();

            if (tableExists == 0)
            {
                // 테이블이 없으면 생성
                var createCmd = new SqlCommand(@"
                        CREATE TABLE [dbo].[AspNetUsers] (
                            [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
                            [UserName] NVARCHAR(256) NULL,
                            [NormalizedUserName] NVARCHAR(256) NULL,
                            [Email] NVARCHAR(256) NULL,
                            [NormalizedEmail] NVARCHAR(256) NULL,
                            [EmailConfirmed] BIT NOT NULL,
                            [PasswordHash] NVARCHAR(MAX) NULL,
                            [SecurityStamp] NVARCHAR(MAX) NULL,
                            [ConcurrencyStamp] NVARCHAR(MAX) NULL,
                            [PhoneNumber] NVARCHAR(MAX) NULL,
                            [PhoneNumberConfirmed] BIT NOT NULL,
                            [TwoFactorEnabled] BIT NOT NULL,
                            [LockoutEnd] DATETIMEOFFSET(7) NULL,
                            [LockoutEnabled] BIT NOT NULL,
                            [AccessFailedCount] INT NOT NULL,
                            [Address] NVARCHAR(MAX) NULL,
                            [FirstName] NVARCHAR(MAX) NULL,
                            [LastName] NVARCHAR(MAX) NULL,
                            [Timezone] NVARCHAR(MAX) NULL,
                            [TenantName] NVARCHAR(MAX) DEFAULT 'Azunt',
                            [RegistrationDate] DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET(),
                            [ShowInDropdown] BIT NULL DEFAULT 0,
                            [RefreshToken] NVARCHAR(MAX) NULL,
                            [RefreshTokenExpiryTime] DATETIME NULL,
                            [DivisionId] BIGINT NULL DEFAULT 0,
                            [TenantId] BIGINT NOT NULL DEFAULT CONVERT(BIGINT, 0)
                        );

                        CREATE NONCLUSTERED INDEX [EmailIndex]
                        ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);

                        CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
                        ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) 
                        WHERE ([NormalizedUserName] IS NOT NULL);
                    ", connection);

                createCmd.ExecuteNonQuery();
                _logger.LogInformation("AspNetUsers table created.");
            }
            else
            {
                // 누락된 컬럼이 있다면 추가
                var expectedColumns = new Dictionary<string, string>
                {
                    ["Address"] = "NVARCHAR(MAX) NULL",
                    ["FirstName"] = "NVARCHAR(MAX) NULL",
                    ["LastName"] = "NVARCHAR(MAX) NULL",
                    ["Timezone"] = "NVARCHAR(MAX) NULL",
                    ["TenantName"] = "NVARCHAR(MAX) DEFAULT 'Azunt'",
                    ["RegistrationDate"] = "DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET()",
                    ["ShowInDropdown"] = "BIT NULL DEFAULT 0",
                    ["RefreshToken"] = "NVARCHAR(MAX) NULL",
                    ["RefreshTokenExpiryTime"] = "DATETIME NULL",
                    ["DivisionId"] = "BIGINT NULL DEFAULT 0",
                    ["TenantId"] = "BIGINT NOT NULL DEFAULT CONVERT(BIGINT, 0)"
                };

                foreach (var kvp in expectedColumns)
                {
                    var columnName = kvp.Key;
                    var columnType = kvp.Value;

                    // 컬럼 존재 여부 확인
                    var cmdCheckColumn = new SqlCommand(@"
                            SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = @ColumnName", connection);
                    cmdCheckColumn.Parameters.AddWithValue("@ColumnName", columnName);

                    int columnExists = (int)cmdCheckColumn.ExecuteScalar();

                    if (columnExists == 0)
                    {
                        // 컬럼 추가
                        var alterCmd = new SqlCommand(
                            $"ALTER TABLE [dbo].[AspNetUsers] ADD [{columnName}] {columnType}", connection);
                        alterCmd.ExecuteNonQuery();

                        _logger.LogInformation($"Column added: {columnName} ({columnType})");
                    }
                }
            }

            // 기본 사용자 데이터 삽입은 외부에서 수행하므로 여기서는 생략
        }
    }

    /// <summary>
    /// Program.cs 또는 Startup.cs에서 호출되는 진입점입니다.
    /// - <c>forMaster == true</c>: 마스터 DB만 처리
    /// - <c>forMaster == false</c>: 테넌트 DB들만 처리
    /// </summary>
    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantSchemaEnhancerEnsureUsersTable>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                throw new InvalidOperationException("Master connection string 'DefaultConnection' is not configured.");
            }

            var enhancer = new TenantSchemaEnhancerEnsureUsersTable(masterConnectionString, logger);

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
            var fallbackLogger = services.GetService<ILogger<TenantSchemaEnhancerEnsureUsersTable>>();
            fallbackLogger?.LogError(ex, "Error while processing AspNetUsers table.");
        }
    }
}
