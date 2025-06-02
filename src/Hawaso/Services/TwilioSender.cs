using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Azunt.Services
{
    /// <summary>
    /// Twilio를 이용한 SMS 발송 서비스
    /// </summary>
    public class TwilioSender : ITwilioSender
    {
        // 설정값을 읽어오기 위한 IConfiguration 주입
        private readonly IConfiguration _configuration;

        public TwilioSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            // 설정 파일에서 실제 Twilio 서비스를 사용할지 여부를 확인
            bool useRealService = _configuration.GetValue<bool>("TwilioSettings:UseRealService");

            // 실제 서비스를 사용할 경우 Twilio API로 전송, 그렇지 않으면 개발용 로그로 저장
            return useRealService
                ? SendRealSmsAsync(phoneNumber, message)
                : SaveSmsToDevelopmentLogAsync(phoneNumber, message);
        }

        // 실제 Twilio API를 이용해 SMS 전송
        private async Task SendRealSmsAsync(string phoneNumber, string message)
        {
            // 필수 설정값들을 가져옴 (없으면 예외 발생)
            string accountSid = _configuration["AppKeys:TwilioAccountSid"] ?? throw new InvalidOperationException("Missing AppKeys:TwilioAccountSid");
            string authToken = _configuration["AppKeys:TwilioAuthToken"] ?? throw new InvalidOperationException("Missing AppKeys:TwilioAuthToken");
            string fromPhoneNumber = _configuration["AppKeys:TwilioPhoneNumber"] ?? throw new InvalidOperationException("Missing AppKeys:TwilioPhoneNumber");

            // Twilio API 클라이언트 초기화
            TwilioClient.Init(accountSid, authToken);

            // 메시지 전송
            await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(fromPhoneNumber),
                body: message
            );
        }

        // 개발 환경에서는 SMS를 실제로 보내지 않고 로그 파일에 저장
        private async Task SaveSmsToDevelopmentLogAsync(string phoneNumber, string message)
        {
            // 개발용 로그 저장 경로 설정값 읽기
            string? developmentSavePath = _configuration["TwilioSettings:DevelopmentSavePath"];
            if (string.IsNullOrWhiteSpace(developmentSavePath))
            {
                throw new InvalidOperationException("TwilioSettings:DevelopmentSavePath is not configured.");
            }

            // 로그 내용 구성
            var logEntry = new StringBuilder();
            logEntry.AppendLine("===== SMS Development Log =====");
            logEntry.AppendLine($"Timestamp : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            logEntry.AppendLine($"To        : {phoneNumber}");
            logEntry.AppendLine($"Message   : {message}");
            logEntry.AppendLine();

            // 로그 저장 경로가 없으면 생성
            var directory = Path.GetDirectoryName(developmentSavePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 로그 파일에 내용 추가
            await File.AppendAllTextAsync(developmentSavePath, logEntry.ToString());
        }
    }
}
