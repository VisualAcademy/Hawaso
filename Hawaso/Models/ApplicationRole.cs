using Microsoft.AspNetCore.Identity;

namespace Hawaso.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
    }
}
