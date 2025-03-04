using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Apis.Documents
{
    public class DocumentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
