using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class VendorsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
