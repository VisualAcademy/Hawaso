using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Azunt.Services
{
    public class TwilioSender_Twilio : ITwilioSender
    {
        public Task SendSmsAsync(string phoneNumber, string message)
        {
            var accountSid = "";
            var authToken = "";
            var twilioPhoneNumber = "";

            TwilioClient.Init(accountSid, authToken);

            return MessageResource.CreateAsync(
              to: new PhoneNumber(phoneNumber),
              from: new PhoneNumber(twilioPhoneNumber),
              body: message);
        }
    }
}
