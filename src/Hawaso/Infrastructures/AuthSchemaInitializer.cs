using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azunt.Infrastructures.Tenants;
using Hawaso.Infrastructures;

namespace Azunt.Infrastructures
{
    public static class AuthSchemaInitializer
    {
        public static void Initialize(IServiceProvider services)
        {
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("AuthSchemaInitializer");

            InitializeRolesTable(services, logger, forMaster: true);
            InitializeRolesTable(services, logger, forMaster: false);
        }

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
    }
}
