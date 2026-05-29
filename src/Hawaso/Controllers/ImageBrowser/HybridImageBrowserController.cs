using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers.ImageBrowser
{
    public class HybridImageBrowserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
