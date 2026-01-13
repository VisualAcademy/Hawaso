using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class UserSignaturesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
