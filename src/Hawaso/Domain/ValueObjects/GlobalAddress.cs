using Azunt.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Hawaso.Domain.ValueObjects;

[Owned] // EF Core에 "소유된 타입"이라고 알림
public sealed class GlobalAddress : AddressBase
{
    public string StateOrProvince { get; set; } // ISO 3166-2 등
    public string County { get; set; }          // GB 등 일부 국가
    public string TimeZoneId { get; set; }      // 선택
}
