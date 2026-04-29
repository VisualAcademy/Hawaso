using Azunt.Web.Data;
using Azunt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Azunt.Web.Services;

public class SitePageRouteSyncService
{
    private readonly IEnumerable<EndpointDataSource> _endpointDataSources;
    private readonly ApplicationDbContext _context;

    public SitePageRouteSyncService(
        IEnumerable<EndpointDataSource> endpointDataSources,
        ApplicationDbContext context)
    {
        _endpointDataSources = endpointDataSources;
        _context = context;
    }

    public async Task<int> SyncAsync()
    {
        var now = DateTime.UtcNow;

        var currentEndpoints = _endpointDataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .SelectMany(CreateSitePageCandidates)
            .GroupBy(x => new { x.RoutePattern, x.HttpMethod })
            .Select(g => g.First())
            .ToList();

        var existingPages = await _context.SitePages.ToListAsync();

        foreach (var page in existingPages)
        {
            page.IsEndpointActive = false;
        }

        var changedCount = 0;

        foreach (var endpoint in currentEndpoints)
        {
            var existing = existingPages.FirstOrDefault(x =>
                x.RoutePattern == endpoint.RoutePattern &&
                x.HttpMethod == endpoint.HttpMethod);

            if (existing == null)
            {
                var maxSortOrder = await _context.SitePages
                    .Select(x => (int?)x.SortOrder)
                    .MaxAsync() ?? 0;

                endpoint.SortOrder = maxSortOrder + 10;
                endpoint.PageNumber = null;
                endpoint.CreatedAtUtc = now;
                endpoint.LastSyncedAtUtc = now;
                endpoint.IsEndpointActive = true;

                _context.SitePages.Add(endpoint);
                changedCount++;
            }
            else
            {
                existing.DisplayName = endpoint.DisplayName;
                existing.AllowAnonymous = endpoint.AllowAnonymous;
                existing.RequiredRoles = endpoint.RequiredRoles;
                existing.RequiredPolicy = endpoint.RequiredPolicy;
                existing.IsEndpointActive = true;
                existing.LastSyncedAtUtc = now;
                existing.UpdatedAtUtc = now;

                changedCount++;
            }
        }

        await _context.SaveChangesAsync();

        return changedCount;
    }

    private static IEnumerable<SitePage> CreateSitePageCandidates(RouteEndpoint endpoint)
    {
        var routePattern = endpoint.RoutePattern.RawText;

        if (string.IsNullOrWhiteSpace(routePattern))
        {
            routePattern = endpoint.RoutePattern.ToString();
        }

        if (string.IsNullOrWhiteSpace(routePattern))
        {
            yield break;
        }

        if (routePattern.StartsWith("_blazor", StringComparison.OrdinalIgnoreCase) ||
            routePattern.StartsWith("css/", StringComparison.OrdinalIgnoreCase) ||
            routePattern.StartsWith("js/", StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        var httpMethods = endpoint.Metadata
            .GetMetadata<IHttpMethodMetadata>()?
            .HttpMethods
            .ToList();

        if (httpMethods == null || httpMethods.Count == 0)
        {
            httpMethods = ["GET"];
        }

        var authorizeData = endpoint.Metadata
            .GetOrderedMetadata<IAuthorizeData>()
            .ToList();

        var allowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null;

        var controllerAction = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

        var displayName = controllerAction != null
            ? $"{controllerAction.ControllerName}.{controllerAction.ActionName}"
            : endpoint.DisplayName;

        var roles = string.Join(",",
            authorizeData
                .Select(x => x.Roles)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct());

        var policies = string.Join(",",
            authorizeData
                .Select(x => x.Policy)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct());

        foreach (var method in httpMethods)
        {
            yield return new SitePage
            {
                RoutePattern = NormalizeRoute(routePattern),
                HttpMethod = method,
                DisplayName = displayName,
                PageTitle = displayName,
                AllowAnonymous = allowAnonymous,
                RequiredRoles = string.IsNullOrWhiteSpace(roles) ? null : roles,
                RequiredPolicy = string.IsNullOrWhiteSpace(policies) ? null : policies,
                IsPublic = allowAnonymous || authorizeData.Count == 0,
                IsVisibleInDashboard = true,
                IsEndpointActive = true
            };
        }
    }

    private static string NormalizeRoute(string route)
    {
        route = route.Trim();

        if (route == "")
        {
            return "/";
        }

        return route;
    }
}