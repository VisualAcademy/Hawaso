using Microsoft.Extensions.Configuration;

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
            return new TenantPermanentDelete
            {
                ConnectionString = _configuration.GetConnectionString("TenantDbConnection")
            };
        }
    }
}
