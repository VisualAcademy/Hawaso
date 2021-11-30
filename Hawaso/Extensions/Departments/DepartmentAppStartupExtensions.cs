using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VisualAcademy.Models.Departments;

namespace Hawaso.Models
{
    public static class DepartmentAppStartupExtensions
    {
        public static void AddDependencyInjectionContainerForDepartmentApp(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<DepartmentAppDbContext>(options => options.UseSqlServer(connectionString)
            //.EnableSensitiveDataLogging()
            , ServiceLifetime.Transient);
            services.AddTransient<IDepartmentRepository, DepartmentRepository>();
        }
    }
}
