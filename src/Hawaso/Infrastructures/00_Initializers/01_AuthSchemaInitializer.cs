using Azunt.Web.Infrastructures.Auth;

namespace Azunt.Web.Infrastructures._Initializers;

/// <summary>
/// 인증/인가 관련 테이블 및 초기 사용자 생성을 담당하는 초기화 클래스입니다.
/// - 마스터 DB 및 테넌트 DB에 AspNetRoles, AspNetUsers 테이블을 생성/보강합니다.
/// - 기본 사용자 계정을 삽입합니다.
/// </summary>
public static class AuthSchemaInitializer
{
    /// <summary>
    /// 초기화 진입점입니다. Program.cs 또는 Startup.cs에서 호출됩니다.
    /// - 역할 테이블 초기화
    /// - 사용자 테이블 초기화
    /// - 기본 사용자 생성
    /// </summary>
    /// <param name="services">DI 컨테이너 서비스 프로바이더</param>
    public static void Initialize(IServiceProvider services)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("AuthSchemaInitializer");

        // 마스터 DB에 AspNetRoles 테이블 생성 및 보강
        InitializeRolesTable(services, logger, forMaster: true);

        // 마스터 DB에 AspNetUsers 테이블 생성 및 보강
        InitializeUsersTable(services, logger, forMaster: true);

        // 마스터 DB에 기본 사용자 삽입 (예: admin 계정)
        InitializeDefaultUsers(services, logger);
    }

    /// <summary>
    /// AspNetRoles 테이블을 초기화합니다.
    /// </summary>
    /// <param name="services">서비스 프로바이더</param>
    /// <param name="logger">로거</param>
    /// <param name="forMaster">마스터 DB 대상 여부</param>
    private static void InitializeRolesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureRolesTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 AspNetRoles 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 AspNetRoles 테이블 초기화 중 오류 발생");
        }
    }

    /// <summary>
    /// AspNetUsers 테이블을 초기화합니다.
    /// </summary>
    /// <param name="services">서비스 프로바이더</param>
    /// <param name="logger">로거</param>
    /// <param name="forMaster">마스터 DB 대상 여부</param>
    private static void InitializeUsersTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureUsersTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 AspNetUsers 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 AspNetUsers 테이블 초기화 중 오류 발생");
        }
    }

    /// <summary>
    /// 기본 사용자(예: 관리자 계정)를 삽입합니다.
    /// </summary>
    /// <param name="services">서비스 프로바이더</param>
    /// <param name="logger">로거</param>
    private static void InitializeDefaultUsers(IServiceProvider services, ILogger logger)
    {
        try
        {
            using (var scope = services.CreateScope())
            {
                TenantSchemaEnhancerEnsureDefaultUsers.RunAsync(scope.ServiceProvider).GetAwaiter().GetResult();
                logger.LogInformation("기본 사용자 초기화 완료");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "기본 사용자 초기화 중 오류 발생");
        }
    }
}
