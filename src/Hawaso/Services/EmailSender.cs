using Microsoft.AspNetCore.Identity.UI.Services;

namespace Hawaso.Services;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine(htmlMessage);
        return Task.CompletedTask;
    }
}
