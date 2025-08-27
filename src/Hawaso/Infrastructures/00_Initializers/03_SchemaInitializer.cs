using Azunt.ApplicantTypeManagement;
using Azunt.Infrastructures.Tenants;
using Azunt.Web.Infrastructures.All;
using Azunt.Web.Infrastructures.Tenants;

namespace Hawaso.Infrastructures.Initializers;

public static class SchemaInitializer
{
    public static void Initialize(IServiceProvider services)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("SchemaInitializer");

        var config = services.GetRequiredService<IConfiguration>();
        var masterConnectionString = config.GetConnectionString("DefaultConnection");

        // ApplicantTypes 먼저
        InitializeApplicantTypesTable(services, logger, forMaster: true);
        InitializeLicenseTypesTable(services, logger, forMaster: true);
        InitializeContactTypesTable(services, logger, forMaster: true);
        InitializeAllsTable(services, logger, forMaster: true);
        InitializeLicenseStatusesTable(services, logger, forMaster: true);
        InitializeSmsLogsTable(services, logger, forMaster: true);
        InitializeAllowedIpRangesTable(services, logger, forMaster: true);
        InitializeIncidentTableEnhancer(services, logger, forMaster: true);
    }

    private static void InitializeApplicantTypesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            // NuGet의 비동기 메서드를 동기 파이프라인에 맞춰 호출
            ApplicantTypesTableBuilder.RunAsync(services, forMaster).GetAwaiter().GetResult();
            logger.LogInformation($"{target}의 ApplicantTypes 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 ApplicantTypes 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeLicenseTypesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureLicenseTypesTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 LicenseTypes 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 LicenseTypes 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeContactTypesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureContactTypesTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 ContactTypes 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 ContactTypes 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeAllsTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureAllsTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 Alls 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 Alls 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeLicenseStatusesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureVendorLicenseStatusesTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 LicenseStatuses 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 LicenseStatuses 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeSmsLogsTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureSmsLogsTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 SmsLogs 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 SmsLogs 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeAllowedIpRangesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureAllowedIpRangesTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 AllowedIpRanges 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 AllowedIpRanges 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeIncidentTableEnhancer(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            Azunt.IncidentManagement.IncidentTableEnhancer.Run(services, forMaster);
            logger.LogInformation($"{target}의 Incidents 테이블 확장 작업 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 Incidents 테이블 확장 작업 중 오류 발생");
        }
    }
}
