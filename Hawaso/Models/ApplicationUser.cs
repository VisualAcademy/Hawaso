using Microsoft.AspNetCore.Identity;

namespace Hawaso.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string Address { get; set; }
    }
}
