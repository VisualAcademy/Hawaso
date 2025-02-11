using Microsoft.EntityFrameworkCore;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class VendorsClientsDbContextFactory : IDbContextFactory<VendorsClientsDbContext>
    {
        private readonly IConfiguration _configuration;

        public VendorsClientsDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public VendorsClientsDbContext CreateDbContext(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _configuration.GetConnectionString("TenantDbConnection");
            }

            var optionsBuilder = new DbContextOptionsBuilder<VendorsClientsDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new VendorsClientsDbContext(optionsBuilder.Options);
        }

        public VendorsClientsDbContext CreateDbContext()
        {
            var defaultConnectionString = _configuration.GetConnectionString("TenantDbConnection");
            return CreateDbContext(defaultConnectionString);
        }
    }
}
