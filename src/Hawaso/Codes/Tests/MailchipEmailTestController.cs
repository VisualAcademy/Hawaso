using All.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Codes.Tests
{
    [Authorize(Roles = "Administrators")]
    public class MailchipEmailTestController : Controller
    {
        private readonly IMailchimpEmailSender _mailchimpEmailSender;

        public MailchipEmailTestController(IMailchimpEmailSender mailchimpEmailSender)
        {
            this._mailchimpEmailSender = mailchimpEmailSender;
        }

        public async Task<IActionResult> SendMailchimpEmail()
        {
            //await _mailchimpEmailSender.SendEmailAsync(
            //    "yourtestemail@youremail.com", "Mailchimp Email Send Test", "This is a Mailchimp test email.");
            await Task.CompletedTask;
            return Content("Mailchimp email sent successfully!");
        }
    }
}
