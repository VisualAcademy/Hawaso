using Microsoft.EntityFrameworkCore;

namespace Hawaso.Models
{
    public class ManufacturerDbContext : DbContext
    {
        public ManufacturerDbContext()
        {
            // Empty
        }

        public ManufacturerDbContext(DbContextOptions<ManufacturerDbContext> options)
            : base(options)
        {
            // 공식과 같은 코드
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Empty
        }

        public DbSet<Manufacturer> Manufacturers { get; set; }
    }
}
