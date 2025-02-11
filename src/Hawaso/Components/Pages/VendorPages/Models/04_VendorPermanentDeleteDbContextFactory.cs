using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class VendorPermanentDeleteDbContextFactory : IDbContextFactory<VendorPermanentDeleteDbContext>
    {
        private readonly IConfiguration _configuration;

        public VendorPermanentDeleteDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public VendorPermanentDeleteDbContext CreateDbContext(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _configuration.GetConnectionString("TenantDbConnection");
            }

            var optionsBuilder = new DbContextOptionsBuilder<VendorPermanentDeleteDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new VendorPermanentDeleteDbContext(optionsBuilder.Options);
        }

        public VendorPermanentDeleteDbContext CreateDbContext()
        {
            var defaultConnectionString = _configuration.GetConnectionString("TenantDbConnection");
            return CreateDbContext(defaultConnectionString);
        }
    }
}
