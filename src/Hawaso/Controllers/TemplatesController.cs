using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class TemplatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
