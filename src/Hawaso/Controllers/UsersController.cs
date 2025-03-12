using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DisableTwoFactorForTenant()
        {
            //var currentUser = await userManager.GetUserAsync(User);
            //if (currentUser == null)
            //{
            //    return NotFound("User not found");
            //}

            //var tenantId = currentUser.TenantID;
            //var usersInTenant = userManager.Users.Where(u => u.TenantID == tenantId);

            //foreach (var user in usersInTenant)
            //{
            //    user.TwoFactorEnabled = false;
            //    var result = await userManager.UpdateAsync(user);
            //    if (!result.Succeeded)
            //    {
            //        // 로그 기록 또는 에러 처리
            //    }
            //}

            return Ok("Two-factor authentication disabled for all users in the tenant.");
        }
    }
}
