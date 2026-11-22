using System;
using System.Collections.Generic;
using System.Linq;

namespace Hawaso.Models.AzuntDashboard;

public sealed record DashboardNavigationItem(
    string Key,
    string Text,
    string Icon,
    string Href,
    IReadOnlyList<DashboardNavigationItem>? Children = null,
    bool ExpandedByDefault = false,
    string? Group = null,
    string? Policy = null);

public sealed class DashboardNavigationNodeViewModel
{
    public string Key { get; init; } = "";
    public string Text { get; init; } = "";
    public string Icon { get; init; } = "";
    public string Href { get; init; } = "";
    public int Depth { get; init; }
    public bool IsCurrent { get; init; }
    public bool ContainsCurrent { get; init; }
    public bool IsExpanded { get; init; }
    public string? Group { get; init; }
    public IReadOnlyList<DashboardNavigationNodeViewModel> Children { get; init; } = [];
}

public sealed class DashboardNavigationViewModel
{
    public IReadOnlyList<DashboardNavigationNodeViewModel> Items { get; init; } = [];
}

/// <summary>
/// AzuntSiteStarter navigation followed by every Hawaso dashboard group.
/// No database or authorization rules are changed; App Logs remains policy-gated.
/// </summary>
public static class DashboardNavigationCatalog
{
    private static readonly IReadOnlyList<DashboardNavigationItem> Navigation =
    [
        new("overview", "Overview", "home", "/dashboard", Group: "AZUNT STARTER"),
        new("resources", "Resources", "resources", "/dashboard/resources", [
            new("app-services", "App services", "app-service", "/dashboard/resources/app-services"),
            new("databases", "Databases", "database", "/dashboard/resources/databases"),
            new("storage", "Storage", "storage", "/dashboard/resources/storage")
        ], Group: "AZUNT STARTER"),
        new("management", "Management", "settings", "/dashboard/management", [
            new("identity", "Identity", "identity", "/dashboard/management/identity", [
                new("users", "Users", "users", "/dashboard/management/identity/users"),
                new("roles", "Roles", "roles", "/dashboard/management/identity/roles")
            ]),
            new("tenants", "Tenants", "tenants", "/dashboard/management/tenants")
        ], Group: "AZUNT MANAGEMENT"),
        new("monitoring", "Monitoring", "monitoring", "/dashboard/monitoring", [
            new("activity", "Activity log", "activity", "/dashboard/monitoring/activity"),
            new("health", "Service health", "health", "/dashboard/monitoring/health")
        ], Group: "AZUNT MANAGEMENT"),
        new("documentation", "Documentation", "docs", "#", Group: "AZUNT LINKS"),
        new("protected-docs", "Protected docs", "shield", "#", Group: "AZUNT LINKS"),
        new("account", "Account", "account", "/Identity/Account/Manage", Group: "AZUNT LINKS"),
        new("hawaso-dashboard", "Dashboard", "dashboard", "/Dashboard", Group: "HAWASO · CORE"),
        new("hawaso-demo", "데모", "docs", "/CodeSnippets/CurrentUserData1", [
            new("demo-1", "섹션 1", "resources", "/CodeSnippets/CurrentUserData1", [
                new("example-1", "예제 1", "docs", "/CodeSnippets/CurrentUserData1"),
                new("example-2", "예제 2", "docs", "/CodeSnippets/CurrentUserData2"),
                new("example-3", "예제 3", "docs", "/CodeSnippets/CurrentUserData3")
            ]),
            new("demo-2", "섹션 2", "resources", "/CodeSnippets/CurrentUserData1", [
                new("example-2-1", "예제 1", "docs", "/CodeSnippets/CurrentUserData1"),
                new("example-2-2", "예제 2", "docs", "/CodeSnippets/CurrentUserData2"),
                new("example-2-3", "예제 3", "docs", "/CodeSnippets/CurrentUserData3")
            ])
        ], Group: "HAWASO · CORE"),
        new("hawaso-board", "게시판", "docs", "/DotNetNote", [
            new("board-crud", "CRUD", "resources", "/MachineTypes", [
                new("machine-types", "MachineTypes", "resources", "/MachineTypes"),
                new("manufacturers", "Manufacturers", "resources", "/Manufacturers"),
                new("departments", "Departments", "resources", "/Departments")
            ]),
            new("board-forums", "게시판", "docs", "/DotNetNote", [
                new("notices", "공지사항", "docs", "/Notices"),
                new("uploads", "자료실", "storage", "/Uploads"),
                new("libraries", "__라이브러리", "docs", "/Libraries"),
                new("briefinglogs", "__브리핑 로그", "activity", "/BriefingLogs"),
                new("replys", "답변형", "docs", "/Replys"),
                new("dotnetnote", "완성형(MVC)", "docs", "/DotNetNote"),
                new("memos", "완성형(Blazor)", "docs", "/Memos"),
                new("archives", "__아카이브", "docs", "/Archives")
            ])
        ], Group: "HAWASO · CORE"),
        new("hawaso-counter", "Counter", "dashboard", "/Counter", Group: "HAWASO · CORE"),
        new("hawaso-customers", "Customers", "users", "/Customers", Group: "HAWASO · CORE"),
        new("hawaso-landing", "Landing", "home", "/Home", Group: "HAWASO · CORE"),
        new("hawaso-sale", "DotNetSale", "products", "/Products", [
            new("sale-categories", "Categories", "products", "/Categories"),
            new("sale-products", "Products", "products", "/Products"),
            new("sale-relations", "CategoriesProducts", "resources", "/CategoriesProducts"),
            new("sale-customers", "Customers", "users", "/Customers")
        ], Group: "HAWASO · SALES"),
        new("hawaso-applogs", "App Logs (Serilog)", "activity", "/AppLogs", Group: "HAWASO · ADMINISTRATIONS", Policy: "GlobalAdministrators"),
        new("hawaso-users-roles", "Users & Roles", "identity", "/Administrations/Roles/RoleList", [
            new("hawaso-roles", "Roles", "roles", "/Administrations/Roles/RoleList", [
                new("hawaso-role-manager", "Role Manager", "roles", "/Administrations/Roles/RoleList")
            ])
        ], Group: "HAWASO · ADMINISTRATIONS"),
        new("hawaso-admin", "Admin", "settings", "/Departments", [
            new("admin-departments", "Departments", "resources", "/Departments"),
            new("admin-manufacturers", "Manufacturers", "resources", "/Manufacturers"),
            new("admin-notices", "Notices", "docs", "/Notices"),
            new("admin-uploads", "Uploads", "storage", "/Uploads"),
            new("admin-replys", "Replys", "docs", "/Replys"),
            new("admin-machinetypes", "Machine Type", "resources", "/MachineTypes"),
            new("admin-logins", "Logins", "identity", "/Logins")
        ], Group: "HAWASO · ADMINISTRATIONS"),
        new("hawaso-snippets", "Code Snippets", "docs", "/CodeSnippets/CurrentUserData1", [
            new("hawaso-current-user", "접속자 정보", "account", "/CodeSnippets/CurrentUserData1", [
                new("user-example-1", "접속자 정보 얻기 1", "docs", "/CodeSnippets/CurrentUserData1"),
                new("user-example-2", "접속자 정보 얻기 2", "docs", "/CodeSnippets/CurrentUserData2"),
                new("user-example-3", "접속자 정보 얻기 3", "docs", "/CodeSnippets/CurrentUserData3")
            ]),
            new("hawaso-current-user-copy", "접속자 정보 (추가 예제)", "account", "/CodeSnippets/CurrentUserData1", [
                new("user-example-copy-1", "접속자 정보 얻기 1", "docs", "/CodeSnippets/CurrentUserData1"),
                new("user-example-copy-2", "접속자 정보 얻기 2", "docs", "/CodeSnippets/CurrentUserData2"),
                new("user-example-copy-3", "접속자 정보 얻기 3", "docs", "/CodeSnippets/CurrentUserData3")
            ])
        ], Group: "HAWASO · ADMINISTRATIONS"),
        new("hawaso-layouts", "Layouts", "wireframe", "/templates/dashboard/layout-static.html", [
            new("layout-static", "Static Navigation", "wireframe", "/templates/dashboard/layout-static.html"),
            new("layout-light", "Light Sidenav", "wireframe", "/templates/dashboard/layout-sidenav-light.html")
        ], Group: "HAWASO · INTERFACE"),
        new("hawaso-pages", "Pages", "docs", "/Identity/Account/Login", [
            new("hawaso-auth", "Authentication", "identity", "/Identity/Account/Login", [
                new("hawaso-login", "Login", "account", "/Identity/Account/Login"),
                new("hawaso-register", "Register", "account", "/Identity/Account/Register"),
                new("hawaso-forgot", "Forgot Password", "account", "/Identity/Account/ForgotPassword")
            ]),
            new("hawaso-errors", "Error", "shield", "/templates/dashboard/401.html", [
                new("hawaso-401", "401 Page", "shield", "/templates/dashboard/401.html"),
                new("hawaso-404", "404 Page", "shield", "/templates/dashboard/404.html"),
                new("hawaso-500", "500 Page", "shield", "/templates/dashboard/500.html")
            ])
        ], Group: "HAWASO · INTERFACE"),
        new("hawaso-charts", "Charts", "monitoring", "/templates/dashboard/charts.html", Group: "HAWASO · ADDONS"),
        new("hawaso-tables", "Tables", "database", "/templates/dashboard/tables.html", Group: "HAWASO · ADDONS")
    ];

    public static DashboardNavigationViewModel Build(string? currentPath, bool canViewAppLogs)
    {
        var normalized = Normalize(currentPath);
        return new DashboardNavigationViewModel
        {
            Items = Navigation
                .Where(x => x.Policy != "GlobalAdministrators" || canViewAppLogs)
                .Select(x => BuildNode(x, normalized, 0))
                .ToArray()
        };
    }

    private static DashboardNavigationNodeViewModel BuildNode(DashboardNavigationItem item, string currentPath, int depth)
    {
        var children = (item.Children ?? [])
            .Select(x => BuildNode(x, currentPath, depth + 1))
            .ToArray();
        var isCurrent = string.Equals(Normalize(item.Href), currentPath, StringComparison.OrdinalIgnoreCase);
        var containsCurrent = isCurrent || children.Any(x => x.ContainsCurrent);
        return new DashboardNavigationNodeViewModel
        {
            Key = item.Key,
            Text = item.Text,
            Icon = item.Icon,
            Href = item.Href,
            Depth = depth,
            IsCurrent = isCurrent,
            ContainsCurrent = containsCurrent,
            IsExpanded = item.ExpandedByDefault || containsCurrent,
            Group = item.Group,
            Children = children
        };
    }

    private static string Normalize(string? path)
    {
        var normalized = string.IsNullOrWhiteSpace(path) ? "/" : path.Trim();
        return normalized.Length > 1 ? normalized.TrimEnd('/').ToLowerInvariant() : normalized.ToLowerInvariant();
    }
}
