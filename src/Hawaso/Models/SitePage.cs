using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Azunt.Web.Models;

public class SitePage
{
    public long Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string RoutePattern { get; set; } = "";

    [MaxLength(50)]
    public string? HttpMethod { get; set; }

    [MaxLength(300)]
    public string? DisplayName { get; set; }

    [MaxLength(200)]
    public string? PageTitle { get; set; }

    public int? PageNumber { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublic { get; set; } = true;

    public bool IsVisibleInDashboard { get; set; } = true;

    [MaxLength(500)]
    public string? RequiredRoles { get; set; }

    [MaxLength(200)]
    public string? RequiredPolicy { get; set; }

    public bool AllowAnonymous { get; set; }

    public bool IsEndpointActive { get; set; } = true;

    public DateTime? LastSyncedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    [NotMapped]
    public string Url
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RoutePattern))
            {
                return "/";
            }

            var route = RoutePattern.Trim();

            if (route == "/")
            {
                return "/";
            }

            return route.StartsWith("/") ? route : "/" + route;
        }
    }
}