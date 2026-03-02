using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hawaso.Pages
{
    [AllowAnonymous]
    public class AccessDeniedDueToIPModel : PageModel
    {
        public string ClientIp { get; set; } = string.Empty;

        public void OnGet()
        {
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            ViewData["ClientIp"] = ClientIp;
        }
    }
}
