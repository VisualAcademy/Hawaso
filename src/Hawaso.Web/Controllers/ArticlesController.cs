using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Web.Controllers
{
    public class ArticlesController : Controller
    {
        public IActionResult Index() => View();
    }
}
