using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Zero.Models
{
    public static class BriefingLogAppExtensions
    {
        public static void AddDiForBriefingLogs(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<BriefingLogAppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);
            services.AddTransient<IBriefingLogRepository, BriefingLogRepository>();
            services.AddTransient<IBriefingLogFileStorageManager, BriefingLogFileStorageManager>();
        }
    }
}
