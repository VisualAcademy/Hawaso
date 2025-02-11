using Microsoft.EntityFrameworkCore;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public static class VendorPermanentDeleteStartupExtensions
    {
        public static void AddDependencyInjectionContainerForVendorPermanentDelete(this IServiceCollection services, string connectionString)
        {
            var create = new VendorPermanentDeleteSchemaEnhancer(connectionString);
            create.EnsureVendorsTableExists();

            services.AddDbContext<VendorPermanentDeleteDbContext>(options => { }, ServiceLifetime.Scoped);
            services.AddScoped<VendorPermanentDeleteDbContextFactory>();
            services.AddSingleton<IUserServicePermanentDelete, UserServicePermanentDelete>();
            services.AddTransient<IVendorRepositoryPermanentDelete, VendorRepositoryPermanentDelete>();
        }
    }
}
