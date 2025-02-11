using Microsoft.EntityFrameworkCore;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class VendorRepository : IVendorRepository
    {
        private readonly VendorsClientsDbContextFactory _dbContextFactory;

        public VendorRepository(VendorsClientsDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task AddAsync(Vendor vendor, string connectionString)
        {
            using var context = _dbContextFactory.CreateDbContext(connectionString);
            await context.Vendors.AddAsync(vendor);
            await context.SaveChangesAsync();
        }

        public async Task<List<Vendor>> GetAllAsync(string connectionString)
        {
            using var context = _dbContextFactory.CreateDbContext(connectionString);
            return await context.Vendors.ToListAsync();
        }

        public async Task UpdateAsync(Vendor vendor, string connectionString)
        {
            using var context = _dbContextFactory.CreateDbContext(connectionString);
            context.Entry(vendor).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task ToggleActiveAsync(long id, string connectionString)
        {
            using var context = _dbContextFactory.CreateDbContext(connectionString);
            var vendor = await context.Vendors.FindAsync(id);
            if (vendor != null)
            {
                vendor.Active = !vendor.Active;
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(long id, string connectionString)
        {
            using var context = _dbContextFactory.CreateDbContext(connectionString);
            var vendor = await context.Vendors.FindAsync(id);
            if (vendor != null)
            {
                context.Vendors.Remove(vendor);
                await context.SaveChangesAsync();
            }
        }
    }
}
