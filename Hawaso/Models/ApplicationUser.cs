using Microsoft.AspNetCore.Identity;

namespace Hawaso.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Address { get; set; }
    }
}
