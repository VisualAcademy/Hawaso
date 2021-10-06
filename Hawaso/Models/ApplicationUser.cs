using Microsoft.AspNetCore.Identity;

namespace Hawaso.Models
{
    /// <summary>
    /// 인증 기능
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string Address { get; set; }
    }
}
