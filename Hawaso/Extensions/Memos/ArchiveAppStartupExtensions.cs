using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hawaso.Models
{
    /// <summary>
    /// 메모앱(ArchiveApp) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리 
    /// </summary>
    public static class ArchiveAppStartupExtensions
    {
        public static void AddDependencyInjectionContainerForArchiveApp(this IServiceCollection services, string connectionString)
        {
            // ArchiveAppDbContext.cs Inject: New DbContext Add
            services.AddDbContext<ArchiveAppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);

            // IArchiveRepository.cs Inject: DI Container에 서비스(리포지토리) 등록 
            services.AddTransient<IArchiveRepository, ArchiveRepository>();

            // 파일 업로드 및 다운로드 서비스(매니저) 등록
            services.AddTransient<IArchiveFileStorageManager, ArchiveFileStorageManager>(); // Local Upload
        }
    }
}
