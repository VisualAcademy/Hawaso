namespace Azunt.Services
{
    public class TwilioSender : ITwilioSender
    {
        public Task SendSmsAsync(string phoneNumber, string message)
        {
            return Task.FromResult(0);
        }
    }
}
