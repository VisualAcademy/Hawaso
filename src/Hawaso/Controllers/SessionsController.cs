using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class SessionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
