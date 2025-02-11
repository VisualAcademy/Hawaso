namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Tenant GetUserNotCached()
        {
            return new Tenant
            {
                ConnectionString = _configuration.GetConnectionString("TenantDbConnection")
            };
        }
    }
}
