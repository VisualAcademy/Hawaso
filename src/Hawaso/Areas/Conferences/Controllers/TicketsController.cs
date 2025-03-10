using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Areas.Conferences.Controllers
{
    public class TicketsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
