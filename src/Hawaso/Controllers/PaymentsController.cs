using Azunt.Web.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;

namespace Azunt.Web.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly StripeSettings _stripeSettings;

        public PaymentsController(IOptions<StripeSettings> stripeSettings)
        {
            _stripeSettings = stripeSettings.Value;
        }

        // GET: /Payments/TestCheckout
        [HttpGet]
        public IActionResult TestCheckout()
        {
            // 1) Checkout Session 생성 옵션 (Non-recurring, 일회성 결제)
            var options = new SessionCreateOptions
            {
                Mode = "payment",   // 구독이 아닌 단일 결제
                SuccessUrl = Url.Action(
                    "Success", "Payments", null, Request.Scheme)!,
                CancelUrl = Url.Action(
                    "Cancel", "Payments", null, Request.Scheme)!,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",      // 일단 usd, 나중에 krw도 가능
                            UnitAmount = 500,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Azunt Test Payment"
                            }
                        }
                    }
                }
            };

            var service = new SessionService();
            var session = service.Create(options);

            // 2) Stripe Checkout 페이지로 Redirect
            return Redirect(session.Url);
        }

        // GET: /Payments/Success
        [HttpGet]
        public IActionResult Success()
        {
            ViewBag.Message = "Stripe 테스트 결제가 성공적으로 완료되었습니다. (실제 돈은 빠져나가지 않았습니다.)";
            return View();
        }

        // GET: /Payments/Cancel
        [HttpGet]
        public IActionResult Cancel()
        {
            ViewBag.Message = "Stripe 테스트 결제가 취소되었습니다.";
            return View();
        }
    }
}
