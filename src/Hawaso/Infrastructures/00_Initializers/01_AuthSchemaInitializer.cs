using Azunt.Web.Infrastructures.Auth;

namespace Azunt.Web.Infrastructures._Initializers;

/// <summary>
/// 인증/인가 관련 테이블 및 초기 사용자 생성을 담당하는 초기화 클래스입니다.
/// - 마스터 DB 및 테넌트 DB에 AspNetRoles, AspNetUsers, AspNetUserRoles 테이블을 생성/보강합니다.
/// - 기본 사용자 계정을 삽입합니다.
/// </summary>
public static class AuthSchemaInitializer
{
    /// <summary>
    /// 초기화 진입점입니다. Program.cs 또는 Startup.cs에서 호출됩니다.
    /// </summary>
    /// <param name="services">DI 컨테이너 서비스 프로바이더</param>
    public static void Initialize(IServiceProvider services)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("AuthSchemaInitializer");

        // 1. AspNetRoles 테이블 생성 및 보강
        InitializeRolesTable(services, logger, forMaster: true);

        // 2. AspNetUsers 테이블 생성 및 보강
        InitializeUsersTable(services, logger, forMaster: true);

        // 3. AspNetUserRoles 테이블 생성 및 보강
        InitializeUserRolesTable(services, logger, forMaster: true);

        // 4. 기본 사용자 생성
        InitializeDefaultUsers(services, logger);

        // 5. AspNetUserClaims 테이블 생성 (추가)
        InitializeUserClaimsTable(services, logger, forMaster: true);

        // 6. AspNetUserLogins 테이블 생성
        InitializeUserLoginsTable(services, logger, forMaster: true);

        // 7. AspNetUserTokens 테이블 생성 및 보강 (토큰 기반 인증을 위한 테이블)
        InitializeUserTokensTable(services, logger, forMaster: true);

        // 8. AspNetRoleClaims 테이블 생성 및 보강 (역할 기반 클레임 정책을 위한 테이블)
        InitializeRoleClaimsTable(services, logger, forMaster: true);

        // 9. __EFMigrationsHistory 테이블 생성 (마이그레이션 기록용, EF Core 전용 메타 테이블)
        InitializeMigrationsHistoryTable(services, logger, forMaster: true);
    }

    private static void InitializeRolesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureRolesTable.Run(services, forMaster);
            logger.LogInformation($"[01-01] {target}의 AspNetRoles 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[01-01] {target}의 AspNetRoles 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeUsersTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureUsersTable.Run(services, forMaster);
            logger.LogInformation($"[01-02] {target}의 AspNetUsers 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[01-02] {target}의 AspNetUsers 테이블 초기화 중 오류 발생");
        }
    }

    /// <summary>
    /// AspNetUserRoles 테이블을 초기화합니다.
    /// </summary>
    private static void InitializeUserRolesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureUserRolesTable.Run(services, forMaster);
            logger.LogInformation($"[01-03] {target}의 AspNetUserRoles 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[01-03] {target}의 AspNetUserRoles 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeDefaultUsers(IServiceProvider services, ILogger logger)
    {
        try
        {
            using (var scope = services.CreateScope())
            {
                TenantSchemaEnhancerEnsureDefaultUsers.RunAsync(scope.ServiceProvider).GetAwaiter().GetResult();
                logger.LogInformation("[01-04] 기본 사용자 초기화 완료");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[01-04] 기본 사용자 초기화 중 오류 발생");
        }
    }

    private static void InitializeUserClaimsTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureUserClaimsTable.Run(services, forMaster);
            logger.LogInformation($"[01-05] {target}의 AspNetUserClaims 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[01-05] {target}의 AspNetUserClaims 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeUserLoginsTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureUserLoginsTable.Run(services, forMaster);
            logger.LogInformation($"[01-06] {target}의 AspNetUserLogins 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[01-06] {target}의 AspNetUserLogins 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeUserTokensTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureUserTokensTable.Run(services, forMaster);
            logger.LogInformation($"[01-07] {target}의 AspNetUserTokens 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[01-07] {target}의 AspNetUserTokens 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeRoleClaimsTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureRoleClaimsTable.Run(services, forMaster);
            logger.LogInformation($"[01-08] {target}의 AspNetRoleClaims 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[01-08] {target}의 AspNetRoleClaims 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeMigrationsHistoryTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureMigrationsHistoryTable.Run(services, forMaster);
            logger.LogInformation($"[01-09] {target}의 __EFMigrationsHistory 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[01-09] {target}의 __EFMigrationsHistory 테이블 초기화 중 오류 발생");
        }
    }
}
