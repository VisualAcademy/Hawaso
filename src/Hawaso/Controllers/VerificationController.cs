using All.Models.ManageViewModels;
using All.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    [Authorize]
    public class VerificationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;

        public VerificationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            //_smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<VerificationController>();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddPhoneNumber()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddPhoneNumber(AddPhoneNumberViewModel model)
        {

            return View();
        }

        [HttpGet]
        public IActionResult VerifyPhoneNumber()
        {
            return View();
        }


        [HttpPost]
        public IActionResult VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {

            return View();
        }

        [HttpPost]
        public IActionResult RemovePhoneNumber()
        {
            return RedirectToAction(nameof(Index)); 
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}
