using Azunt.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Hawaso.Domain.ValueObjects;

[Owned] // EF Core에 "소유된 타입"이라고 알림
public sealed class GlobalAddress : AddressBase
{
    // ISO 3166-2 등 (주/도 코드)
    public string StateOrProvince { get; set; } = string.Empty;

    // GB 등 일부 국가에서 사용 (선택)
    public string? County { get; set; }

    // 선택: 시스템 저장용 TimeZone ID
    public string? TimeZoneId { get; set; }
}
