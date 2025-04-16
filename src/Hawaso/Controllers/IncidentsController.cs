using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class IncidentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ReportGeneration(int id)
        {
            return View();
        }
    }
}
