using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers.Applications
{
    public class ApplicationFormDataViewerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
