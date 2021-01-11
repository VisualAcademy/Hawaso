using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MachineTypeApp.Models
{
    /// <summary>
    /// MachineTypeApp 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리 
    /// </summary>
    public static class MachineTypeAppExtensions
    {
        public static void AddDependencyInjectionContainerForMachineTypeApp(this IServiceCollection services, string connectionString)
        {
            // MachineTypeDbContext.cs Inject: New DbContext Add
            services.AddDbContext<MachineTypeDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Transient);

            // IMachineTypeRepository.cs Inject: DI Container에 서비스(리포지토리) 등록 
            services.AddTransient<IMachineTypeRepository, MachineTypeRepository>();
        }
    }
}
