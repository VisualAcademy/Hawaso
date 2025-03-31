using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class ApplicationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
