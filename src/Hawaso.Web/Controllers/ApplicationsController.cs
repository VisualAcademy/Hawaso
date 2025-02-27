using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Web.Controllers
{
    public class ApplicationsController : Controller
    {
        public IActionResult Index()
        {
            //return Content("Applications Index");
            return View();
        }
    }
}
