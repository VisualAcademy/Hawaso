namespace Hawaso.Models
{
    /// <summary>
    /// 메모앱(MemoApp) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리 
    /// </summary>
    public static class PurgeAppStartupExtensions
    {
        public static void AddDependencyInjectionContainerForPurgeApp(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<PurgeAppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);

            services.AddTransient<IPurgeRepository, PurgeRepository>();

            // 파일 업로드 및 다운로드 서비스(매니저) 등록
            services.AddTransient<IPurgeFileStorageManager, PurgeFileStorageManager>(); // Local Upload
        }
    }
}
