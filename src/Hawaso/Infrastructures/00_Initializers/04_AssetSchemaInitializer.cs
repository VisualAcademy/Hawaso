using Azunt.FileManagement;
using Azunt.Infrastructures.Tenants;
using Azunt.Web.Infrastructures.Assets.Tenants;

namespace Hawaso.Infrastructures.Initializers;

public static class AssetSchemaInitializer
{
    public static void Initialize(IServiceProvider services)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("AssetSchemaInitializer");

        var config = services.GetRequiredService<IConfiguration>();
        var masterConnectionString = config.GetConnectionString("DefaultConnection");

        // 테이블마다 forMaster 지정 (유연하게)
        InitializeProjectsMachinesTable(services, logger, forMaster: true); // 또는 false
        InitializeManufacturersTable(services, logger, forMaster: true);
        InitializeInstallsTable(services, logger, forMaster: true);

        InitializeMediaThemesTable(services, logger, forMaster: true);
        InitializeFilesTable(services, logger, forMaster: true);

        InitializeReasonsTable(services, logger, forMaster: true);
    }

    private static void InitializeProjectsMachinesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureProjectsMachinesTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 ProjectsMachines 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 ProjectsMachines 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeManufacturersTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureManufacturersTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 Manufacturers 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 Manufacturers 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeInstallsTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            TenantSchemaEnhancerEnsureInstallsTable.Run(services, forMaster);
            logger.LogInformation($"{target}의 Installs 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 Installs 테이블 초기화 중 오류 발생");
        }
    }




    private static void InitializeMediaThemesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            Azunt.MediaThemeManagement.MediaThemesTableBuilder.Run(services, forMaster);
            logger.LogInformation($"{target}의 MediaThemes 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 MediaThemes 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeFilesTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            FilesTableBuilder.Run(services, forMaster);
            logger.LogInformation($"{target}의 Files 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 Files 테이블 초기화 중 오류 발생");
        }
    }

    private static void InitializeReasonsTable(IServiceProvider services, ILogger logger, bool forMaster)
    {
        string target = forMaster ? "마스터 DB" : "테넌트 DB";

        try
        {
            Azunt.ReasonManagement.ReasonsTableBuilder.Run(services, forMaster); // Azunt.ReasonManagement.dll
            logger.LogInformation($"{target}의 Reasons 테이블 초기화 완료");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{target}의 Reasons 테이블 초기화 중 오류 발생");
        }
    }
}

