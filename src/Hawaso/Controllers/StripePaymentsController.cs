using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class StripePaymentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
