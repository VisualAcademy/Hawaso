using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hawaso.Controllers
{
    public class AboutController : Controller
    {
        private readonly ILogger<AboutController> _logger;

        public AboutController(ILogger<AboutController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("About page visited at {Time}", DateTime.UtcNow);

            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading About page");

                // 사용자에게는 일반 메시지
                return StatusCode(500);
            }
        }
    }
}
