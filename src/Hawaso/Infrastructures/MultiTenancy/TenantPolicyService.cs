using System.Collections.Immutable;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Azunt.Web.Infrastructure.MultiTenancy;

/// <summary>
/// 실제 테넌트 정책 로직을 구현하는 서비스.
/// appsettings.json:TenantPolicies 섹션을 읽어서
/// - 편집 권한 강제 허용 여부,
/// - 업로드 체크 우회 여부,
/// - 유저의 테넌트 매니저 여부
/// 를 판별합니다.
/// </summary>
public class TenantPolicyService : ITenantPolicyService
{
    // 편집 권한 강제 허용 테넌트 목록 (대소문자 무시)
    private readonly ImmutableHashSet<string> _editOverride;

    // 업로드 체크 우회 테넌트 목록 (대소문자 무시)
    private readonly ImmutableHashSet<string> _bypassUpload;

    // 매니저 역할명 접미사 (기본값: "Managers")
    private readonly string _managerRoleSuffix;

    // 특정 테넌트에 대한 커스텀 매니저 역할명 매핑
    private readonly IReadOnlyDictionary<string, string> _customManagerRoles;

    public TenantPolicyService(IOptions<TenantPoliciesOptions> options)
    {
        var o = options.Value;
        _editOverride = (o.EditOverrideTenants ?? new())
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        _bypassUpload = (o.BypassUploadCheckTenants ?? new())
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        _managerRoleSuffix = string.IsNullOrWhiteSpace(o.ManagerRoleSuffix)
            ? "Managers"
            : o.ManagerRoleSuffix;

        _customManagerRoles = o.CustomManagerRoles ?? new Dictionary<string, string>();
    }

    public bool IsEditOverrideTenant(string? tenantName) =>
        !string.IsNullOrWhiteSpace(tenantName) && _editOverride.Contains(tenantName!);

    public bool IsBypassUploadCheckTenant(string? tenantName) =>
        !string.IsNullOrWhiteSpace(tenantName) && _bypassUpload.Contains(tenantName!);

    public async Task<(bool isManager, string? resolvedTenantName)> IsTenantManagerAsync(
        IdentityUser user,
        Func<string, Task<bool>> isInRoleAsync,
        IEnumerable<string> knownTenantNames)
    {
        // 1) 커스텀 매니저 역할 우선 확인
        foreach (var (tenant, roleName) in _customManagerRoles)
        {
            if (await isInRoleAsync(roleName)) return (true, tenant);
        }

        // 2) 규칙 기반 역할명(<TenantName><Suffix>) 확인
        foreach (var t in knownTenantNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var roleName = $"{t}{_managerRoleSuffix}";
            if (await isInRoleAsync(roleName)) return (true, t);
        }

        return (false, null);
    }
}
