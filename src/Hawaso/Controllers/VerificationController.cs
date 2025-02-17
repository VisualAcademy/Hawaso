﻿using All.Models.Enums;
using All.Models.ManageViewModels;
using All.Services;

namespace All.Controllers
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }
    }

    [Authorize]
    public class VerificationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ITwilioSender _twilioSender;

        public VerificationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            //ITwilioSender twilioSender,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            //_smsSender = smsSender;
            //_twilioSender = twilioSender;
            _logger = loggerFactory.CreateLogger<VerificationController>();
        }

        // GET: /Verification/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
            };
            return View(model);
        }

        // GET: /Verification/AddPhoneNumber
        [HttpGet]
        public IActionResult AddPhoneNumber() => View();

        // POST: /Verification/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            // 새로운 인증 코드 생성
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);

            // 사용자에게 인증 코드 전송
            //await twilioSender.SendSmsAsync(model.PhoneNumber, "Your security code is: " + code);

            // VerifyPhoneNumber 페이지로 이동하여 인증 코드 입력
            return RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.PhoneNumber });
        }

        // GET: /Verification/VerifyPhoneNumber
        [HttpGet]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            return phoneNumber == null 
                ? View("Error") 
                : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }


        // 휴대폰 번호 인증을 처리하는 POST 메서드
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF 공격을 방지하기 위한 토큰 검증
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            // 모델 유효성 검사 실패 시, 다시 입력 폼을 표시
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 현재 로그인한 사용자 정보를 가져옴
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                // 사용자의 휴대폰 번호를 변경하고 인증 코드 확인
                var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    // 성공 시, 다시 로그인 처리 후 성공 메시지와 함께 리디렉트
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.AddPhoneSuccess });
                }
            }

            // 인증 실패 시, 오류 메시지를 추가하고 입력 폼을 다시 표시
            ModelState.AddModelError(string.Empty, "휴대폰 번호 인증에 실패했습니다.");
            return View(model);
        }

        // POST: /Verification/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            // 현재 로그인한 사용자 정보를 비동기적으로 가져옴
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                // 사용자의 전화번호를 제거(null로 설정)
                var result = await _userManager.SetPhoneNumberAsync(user, null);
                if (result.Succeeded)
                {
                    // 변경된 사용자 정보를 반영하여 다시 로그인 처리
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // 전화번호 제거 성공 메시지를 포함하여 Index 페이지로 리디렉트
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.RemovePhoneSuccess });
                }
            }

            // 오류 발생 시 에러 메시지를 포함하여 Index 페이지로 리디렉트
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        // 2단계 인증 활성화를 처리하는 POST 메서드
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF 공격을 방지하기 위한 토큰 검증
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            // 현재 로그인한 사용자 정보를 가져옴
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                // 사용자의 2단계 인증을 활성화
                await _userManager.SetTwoFactorEnabledAsync(user, true);

                // 인증 정보를 업데이트하여 다시 로그인 처리
                await _signInManager.SignInAsync(user, isPersistent: false);

                // 로그 기록: 2단계 인증 활성화됨
                _logger.LogInformation(1, "사용자가 2단계 인증을 활성화했습니다.");
            }

            // 인증 관리 페이지로 리디렉트
            return RedirectToAction(nameof(Index), "Verification");
        }

        // 2단계 인증 비활성을 처리하는 POST 메서드
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF 공격을 방지하기 위한 토큰 검증
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            // 현재 로그인한 사용자 정보를 가져옴
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                // 사용자의 2단계 인증을 비활성화
                await _userManager.SetTwoFactorEnabledAsync(user, false);

                // 인증 정보를 업데이트하여 다시 로그인 처리
                await _signInManager.SignInAsync(user, isPersistent: false);

                // 로그 기록: 2단계 인증 비활성화됨
                _logger.LogInformation(2, "사용자가 2단계 인증을 비활성화했습니다.");
            }

            // 인증 관리 페이지로 리디렉트
            return RedirectToAction(nameof(Index), "Verification");
        }

        #region Helpers
        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}
