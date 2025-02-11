using Microsoft.EntityFrameworkCore;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class VendorsClientsDbContext : DbContext
    {
        public VendorsClientsDbContext(DbContextOptions<VendorsClientsDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Vendor> Vendors { get; set; }
    }
}
