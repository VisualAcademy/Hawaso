using Microsoft.EntityFrameworkCore;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class VendorPermanentDeleteDbContextFactory
        : IDbContextFactory<VendorPermanentDeleteDbContext>
    {
        private readonly IConfiguration _configuration;

        public VendorPermanentDeleteDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public VendorPermanentDeleteDbContext CreateDbContext(string? connectionString)
        {
            connectionString ??=
                _configuration.GetConnectionString("TenantDbConnection")
                ?? throw new InvalidOperationException(
                    "TenantDbConnection connection string is not configured.");

            var optionsBuilder =
                new DbContextOptionsBuilder<VendorPermanentDeleteDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new VendorPermanentDeleteDbContext(optionsBuilder.Options);
        }

        public VendorPermanentDeleteDbContext CreateDbContext()
        {
            return CreateDbContext(null);
        }
    }
}
