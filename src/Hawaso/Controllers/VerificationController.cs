using All.Models.ManageViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    public class VerificationController : Controller
    {
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
    }
}
