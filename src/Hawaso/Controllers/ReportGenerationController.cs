using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class ReportGenerationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
