using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

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
                connectionString = _configuration.GetConnectionString("TenantDbConnection")
                    ?? throw new InvalidOperationException(
                        "Connection string 'TenantDbConnection' was not found.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<VendorPermanentDeleteDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new VendorPermanentDeleteDbContext(optionsBuilder.Options);
        }

        public VendorPermanentDeleteDbContext CreateDbContext()
        {
            var defaultConnectionString = _configuration.GetConnectionString("TenantDbConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'TenantDbConnection' was not found.");

            return CreateDbContext(defaultConnectionString);
        }
    }
}