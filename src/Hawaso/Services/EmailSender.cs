using System.Net;
using System.Net.Mail;
using System.Text;

namespace Azunt.Services
{
    /// <summary>
    /// 일반 SMTP 기반 이메일 전송 클래스
    /// </summary>
    public class EmailSender : IEmailSender
    {
        // 설정 파일을 통해 구성 정보 주입
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage, bool isBodyHtml = true)
        {
            // 실제 서비스 사용 여부 확인
            bool useRealService = _configuration.GetValue<bool>("EmailSettings:UseRealService");

            // 실제 이메일 전송 또는 개발 로그 저장 선택
            return useRealService
                ? SendRealEmailAsync(email, subject, htmlMessage, isBodyHtml)
                : SaveEmailToDevelopmentLogAsync(email, subject, htmlMessage, isBodyHtml);
        }

        // 실제 SMTP를 이용한 이메일 전송
        private async Task SendRealEmailAsync(string email, string subject, string body, bool isBodyHtml)
        {
            // 필수 SMTP 설정값 가져오기 (없으면 예외)
            string smtpServer = _configuration["AppKeys:SmtpServer"] ?? throw new InvalidOperationException("Missing AppKeys:SmtpServer");
            string smtpUser = _configuration["AppKeys:SmtpUserName"] ?? throw new InvalidOperationException("Missing AppKeys:SmtpUserName");
            string smtpPassword = _configuration["AppKeys:SmtpPassword"] ?? throw new InvalidOperationException("Missing AppKeys:SmtpPassword");
            string smtpEmail = _configuration["AppKeys:SmtpEmail"] ?? throw new InvalidOperationException("Missing AppKeys:SmtpEmail");
            string replyToEmail = _configuration["AppKeys:ReplyToEmail"] ?? throw new InvalidOperationException("Missing AppKeys:ReplyToEmail");
            string replyToName = _configuration["AppKeys:ReplyToName"] ?? throw new InvalidOperationException("Missing AppKeys:ReplyToName");

            // SMTP 클라이언트 설정
            var client = new SmtpClient(smtpServer)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            // 메일 메시지 구성
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpEmail, replyToName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };

            // 수신자 및 회신자 설정
            mailMessage.To.Add(email);
            mailMessage.ReplyToList.Add(new MailAddress(replyToEmail, replyToName));

            // 메일 전송
            await client.SendMailAsync(mailMessage);
        }

        // 개발 환경용: 이메일 전송 로그를 파일로 저장
        private async Task SaveEmailToDevelopmentLogAsync(string email, string subject, string body, bool isBodyHtml)
        {
            string? developmentSavePath = _configuration["EmailSettings:DevelopmentSavePath"];
            if (string.IsNullOrWhiteSpace(developmentSavePath))
            {
                throw new InvalidOperationException("EmailSettings:DevelopmentSavePath is not configured.");
            }

            // 로그 내용 구성
            var logEntry = new StringBuilder();
            logEntry.AppendLine("===== EMAIL Development Log =====");
            logEntry.AppendLine($"Timestamp : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            logEntry.AppendLine($"To        : {email}");
            logEntry.AppendLine($"Subject   : {subject}");
            logEntry.AppendLine($"IsHtml    : {isBodyHtml}");
            logEntry.AppendLine($"Body      : {body}");
            logEntry.AppendLine();

            // 로그 디렉터리 생성
            var directory = Path.GetDirectoryName(developmentSavePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 로그 파일에 저장
            await File.AppendAllTextAsync(developmentSavePath, logEntry.ToString());
        }
    }

    /// <summary>
    /// Mailchimp SMTP를 사용하는 이메일 전송 클래스
    /// </summary>
    public class MailchimpEmailSender : IMailchimpEmailSender
    {
        // 설정 파일을 통해 구성 정보 주입
        private readonly IConfiguration _configuration;

        public MailchimpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message, bool isBodyHtml = true)
        {
            // 실제 서비스 사용 여부 확인
            bool useRealService = _configuration.GetValue<bool>("EmailSettings:UseRealService");

            // 실제 이메일 전송 또는 개발 로그 저장 선택
            return useRealService
                ? SendRealEmailAsync(email, subject, message, isBodyHtml)
                : SaveEmailToDevelopmentLogAsync(email, subject, message, isBodyHtml);
        }

        // 실제 Mailchimp SMTP 서버를 이용한 이메일 전송
        private async Task SendRealEmailAsync(string email, string subject, string body, bool isBodyHtml)
        {
            // 필수 SMTP 설정값 가져오기 (없으면 예외)
            string smtpServer = _configuration["AppKeys:SmtpServer"] ?? throw new InvalidOperationException("Missing AppKeys:SmtpServer");
            string smtpUser = _configuration["AppKeys:SmtpUserName"] ?? throw new InvalidOperationException("Missing AppKeys:SmtpUserName");
            string smtpPassword = _configuration["AppKeys:SmtpPassword"] ?? throw new InvalidOperationException("Missing AppKeys:SmtpPassword");
            string smtpEmail = _configuration["AppKeys:SmtpEmail"] ?? throw new InvalidOperationException("Missing AppKeys:SmtpEmail");
            string replyToEmail = _configuration["AppKeys:ReplyToEmail"] ?? throw new InvalidOperationException("Missing AppKeys:ReplyToEmail");
            string replyToName = _configuration["AppKeys:ReplyToName"] ?? throw new InvalidOperationException("Missing AppKeys:ReplyToName");

            // SMTP 클라이언트 설정
            var client = new SmtpClient(smtpServer)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            // 메일 메시지 구성
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpEmail, replyToName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };

            // 수신자 및 회신자 설정
            mailMessage.To.Add(email);
            mailMessage.ReplyToList.Add(new MailAddress(replyToEmail, replyToName));

            // 메일 전송
            await client.SendMailAsync(mailMessage);
        }

        // 개발 환경용: 이메일 전송 로그를 파일로 저장
        private async Task SaveEmailToDevelopmentLogAsync(string email, string subject, string body, bool isBodyHtml)
        {
            string? developmentSavePath = _configuration["EmailSettings:DevelopmentSavePath"];
            if (string.IsNullOrWhiteSpace(developmentSavePath))
            {
                throw new InvalidOperationException("EmailSettings:DevelopmentSavePath is not configured.");
            }

            // 로그 내용 구성
            var logEntry = new StringBuilder();
            logEntry.AppendLine("===== EMAIL Development Log =====");
            logEntry.AppendLine($"Timestamp : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            logEntry.AppendLine($"To        : {email}");
            logEntry.AppendLine($"Subject   : {subject}");
            logEntry.AppendLine($"IsHtml    : {isBodyHtml}");
            logEntry.AppendLine($"Body      : {body}");
            logEntry.AppendLine();

            // 로그 디렉터리 생성
            var directory = Path.GetDirectoryName(developmentSavePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 로그 파일에 저장
            await File.AppendAllTextAsync(developmentSavePath, logEntry.ToString());
        }
    }
}
