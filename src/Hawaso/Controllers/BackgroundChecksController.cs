using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class BackgroundChecksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
