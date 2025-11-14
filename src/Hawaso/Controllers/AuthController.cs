using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        [HttpGet("/auth/ping")]
        public IActionResult Ping()
        {
            // 여기까지 들어왔다는 것은 이미 인증된 상태
            return Ok();
        }
    }
}
