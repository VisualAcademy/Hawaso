using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class InvestigationsReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
