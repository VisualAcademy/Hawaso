using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class SpeakingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
