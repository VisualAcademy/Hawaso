using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace All.Services
{
    public class TwilioSender_Twilio : ITwilioSender
    {
        public Task SendSmsAsync(string phoneNumber, string message)
        {
            var accountPhone = "";
            var accountSid = "";
            var authToken = "";

            TwilioClient.Init(accountSid, authToken);

            return MessageResource.CreateAsync(
              to: new PhoneNumber(phoneNumber),
              from: new PhoneNumber(accountPhone),
              body: message);
        }
    }
}
