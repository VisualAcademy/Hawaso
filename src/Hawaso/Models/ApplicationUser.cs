﻿using Microsoft.AspNetCore.Identity;

namespace Hawaso.Models;

/// <summary>
/// ASP.NET Core Identity 인증과 권한 
/// 인증 기능
/// ASP.NET Core / Blazor Server 기본 템플릿에는 IdentityUser로 로그인하지만,
/// 추가적인 정보를 저장할 때에는 ApplicationUser로 속성들을 추가해서 사용
/// </summary>
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string Address { get; set; }

    public string? TenantName { get; set; } = "Hawaso"; // 기본값 Hawaso

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}
