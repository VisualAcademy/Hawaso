using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class CareersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
