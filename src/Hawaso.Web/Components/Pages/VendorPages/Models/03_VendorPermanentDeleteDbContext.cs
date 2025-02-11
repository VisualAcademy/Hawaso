using Microsoft.EntityFrameworkCore;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class VendorPermanentDeleteDbContext : DbContext
    {
        public VendorPermanentDeleteDbContext(DbContextOptions<VendorPermanentDeleteDbContext> options) 
            : base(options)
        {
        }

        public DbSet<VendorPermanentDelete> Vendors { get; set; }
    }
}
