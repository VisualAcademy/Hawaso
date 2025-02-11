namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public interface IVendorRepositoryPermanentDelete
    {
        Task AddAsync(VendorPermanentDelete vendor, string connectionString);
        Task<List<VendorPermanentDelete>> GetAllAsync(string connectionString);
        Task UpdateAsync(VendorPermanentDelete vendor, string connectionString);
        Task ToggleActiveAsync(long id, string connectionString);
        Task DeleteAsync(long id, string connectionString);
    }
}
