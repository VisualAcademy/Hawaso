namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public interface IVendorRepository
    {
        Task AddAsync(Vendor vendor, string connectionString);
        Task<List<Vendor>> GetAllAsync(string connectionString);
        Task UpdateAsync(Vendor vendor, string connectionString);
        Task ToggleActiveAsync(long id, string connectionString);
        Task DeleteAsync(long id, string connectionString);
    }
}
