using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hawaso.Models
{
    /// <summary>
    /// 고객사앱(ManufacturerApp) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리
    /// </summary>
    public static class ManufacturerExtensions
    {
        public static void AddDependencyInjectionContainerForManufacturer(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<ManufacturerDbContext>(options => 
                options.UseSqlServer(connectionString), ServiceLifetime.Transient);
            services.AddTransient<IManufacturerRepository, ManufacturerRepository>(); 
        }
    }
}
