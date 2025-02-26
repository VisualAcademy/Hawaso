using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hawaso.Pages
{
    [AllowAnonymous]
    public class AccessDeniedDueToIPModel : PageModel
    {
        public string ClientIp { get; set; }

        public void OnGet()
        {
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            ViewData["ClientIp"] = ClientIp;
        }
    }
}
