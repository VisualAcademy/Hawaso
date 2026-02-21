using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class EmployeePhotosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
