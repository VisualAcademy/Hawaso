using Microsoft.AspNetCore.Identity;

namespace Hawaso.Data;

/// <summary>
/// ASP.NET Core Identity 인증과 권한 
/// 인증 기능
/// ASP.NET Core / Blazor Server 기본 템플릿에는 IdentityUser로 로그인하지만,
/// 추가적인 정보를 저장할 때에는 ApplicationUser로 속성들을 추가해서 사용
/// </summary>
public class ApplicationUser : IdentityUser
{
    // 'FirstName' 프로퍼티는 사용자의 이름(성이 아닌 부분)을 저장합니다. 
    // 이 정보는 선택적이므로 null일 수 있습니다(? 표시).
    public string FirstName { get; set; }

    // 'LastName' 프로퍼티는 사용자의 성을 저장합니다. 
    // 이 정보도 선택적이므로 null일 수 있습니다(? 표시).
    public string LastName { get; set; }

    // 'Timezone' 프로퍼티는 사용자의 시간대를 저장합니다. 
    // 이를 통해 사용자별로 다른 시간대를 처리할 수 있습니다. 이 정보도 선택적입니다(? 표시).
    public string Timezone { get; set; }

    // TODO: 필요한 추가 프로퍼티들을 여기에 선언하세요.
    [PersonalData]
    public string Address { get; set; }

    public string TenantName { get; set; } = "Hawaso"; // 기본값 Hawaso

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}
