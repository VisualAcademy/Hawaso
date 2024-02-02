using Microsoft.Extensions.DependencyInjection;
using Zero.Models;

namespace Hawaso.Extensions.BriefingLogs;

public static class BriefingLogAppExtensions
{
    public static void AddDiForBriefingLogs(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BriefingLogAppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);
        services.AddTransient<IBriefingLogRepository, BriefingLogRepository>();
        services.AddTransient<IBriefingLogFileStorageManager, BriefingLogFileStorageManager>();
    }
}
