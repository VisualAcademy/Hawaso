using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class AnalyticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
