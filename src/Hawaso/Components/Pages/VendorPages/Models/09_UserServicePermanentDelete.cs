namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class UserServicePermanentDelete : IUserServicePermanentDelete
    {
        private readonly IConfiguration _configuration;

        public UserServicePermanentDelete(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TenantPermanentDelete GetUserNotCached()
        {
            var connectionString = _configuration.GetConnectionString("TenantDbConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'TenantDbConnection' is not configured.");
            }

            return new TenantPermanentDelete
            {
                ConnectionString = connectionString
            };
        }
    }
}