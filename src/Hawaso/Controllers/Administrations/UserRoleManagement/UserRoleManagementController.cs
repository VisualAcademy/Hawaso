using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers.Administrations.UserRoleManagement
{
    public class UserRoleManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
