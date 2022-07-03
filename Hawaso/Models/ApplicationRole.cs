using Microsoft.AspNetCore.Identity;

namespace Hawaso.Models;

/// <summary>
/// ASP.NET Core Identity 인증과 권한 
/// 인증 기능
/// 역할 기반 인증을 사용할 때에는 기본 클래스인 IdentityRole을 상속한
/// ApplicationRole에 역할(그릅) 설명 등과 같은 추가 속성들 제공
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string Description { get; set; }
}
