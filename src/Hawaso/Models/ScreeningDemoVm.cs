namespace Hawaso.Models;

public sealed class ScreeningDemoVm
{
    public required string TenantName { get; set; }
    public string? PartnerName { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsGlobalAdmin { get; set; }
}
