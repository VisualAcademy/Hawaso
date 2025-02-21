using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace Hawaso.Services;

public class EmailSender : All.Services.IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message, bool isBodyHtml = true)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}

public class MailchimpEmailSender : All.Services.IMailchimpEmailSender
{
    private const string REPLY_TO_EMAIL = "support@hawaso.com";
    private const string REPLY_TO_NAME = "Hawaso Team";

    private readonly string _smtpServer;
    private readonly string _smtpUserName;
    private readonly string _smtpPassword;
    private readonly string _smtpEmail;

    public MailchimpEmailSender(IConfiguration configuration)
    {
        _smtpServer = configuration["AppKeys:SmtpServer"];
        _smtpUserName = configuration["AppKeys:SmtpUserName"];
        _smtpPassword = configuration["AppKeys:SmtpPassword"];
        _smtpEmail = configuration["AppKeys:SmtpEmail"];
    }

    public async Task SendEmailAsync(string email, string subject, string message, bool isBodyHtml = true)
    {
        using var client = new SmtpClient(_smtpServer);
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(_smtpUserName, _smtpPassword);

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpEmail, REPLY_TO_NAME),
            Subject = subject,
            Body = message,
            IsBodyHtml = isBodyHtml
        };
        mailMessage.To.Add(email);
        mailMessage.ReplyToList.Add(new MailAddress(REPLY_TO_EMAIL, REPLY_TO_NAME));

        await client.SendMailAsync(mailMessage);
    }
}
