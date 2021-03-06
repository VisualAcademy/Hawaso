using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Zero.Models
{
    public static class LibraryAppExtensions
    {
        public static void AddDiForLibries(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<LibraryAppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);
            services.AddTransient<ILibraryRepository, LibraryRepository>();
            services.AddTransient<ILibraryFileStorageManager, LibraryFileStorageManager>();
        }
    }
}
