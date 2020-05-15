using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
