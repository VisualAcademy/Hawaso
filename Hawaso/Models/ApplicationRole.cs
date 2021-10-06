using Microsoft.AspNetCore.Identity;

namespace Hawaso.Models
{
    /// <summary>
    /// 인증 기능
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
    }
}
