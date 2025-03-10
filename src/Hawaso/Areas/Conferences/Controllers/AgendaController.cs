using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Areas.Conferences.Controllers
{
    public class AgendaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
