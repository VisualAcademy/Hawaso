namespace Hawaso.Models
{
    public sealed class ScreeningDemoVm
    {
        public required string TenantName { get; set; }     // 반드시 설정해야 함
        public string? PartnerName { get; set; }             // 없어도 되는 값이면 nullable
        public bool IsAdmin { get; set; }
        public bool IsGlobalAdmin { get; set; }
    }
}
