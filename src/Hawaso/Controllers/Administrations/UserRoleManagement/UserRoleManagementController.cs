using Azunt.Models.UserRoleViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Hawaso.Data;

namespace Hawaso.Controllers.Administrations.UserRoleManagement;

[Authorize(Roles = "Administrators")]
[Route("Administrations/UserRoleManagement")]
public class UserRoleManagementController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext dbContext) : Controller
{
    private static readonly int[] AllowedPageSizes = { 10, 25, 50, 100 };
    private const int DefaultPageSize = 25;

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string search = "",
        int page = 1,
        int pageSize = DefaultPageSize)
    {
        search = (search ?? string.Empty).Trim();
        page = Math.Max(1, page);
        pageSize = AllowedPageSizes.Contains(pageSize)
            ? pageSize
            : DefaultPageSize;

        var usersQuery = dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search;

            // Explicit Identity entity sets avoid conflicts with any
            // application/business entity that may also be named Role/Roles.
            var roleMatchedUserIds =
                from userRole in dbContext.Set<IdentityUserRole<string>>().AsNoTracking()
                join role in dbContext.Set<ApplicationRole>().AsNoTracking()
                    on userRole.RoleId equals role.Id
                where role.Name != null && role.Name.Contains(searchTerm)
                select userRole.UserId;

            usersQuery = usersQuery.Where(user =>
                (user.Email != null && user.Email.Contains(searchTerm)) ||
                (user.UserName != null && user.UserName.Contains(searchTerm)) ||
                (user.FirstName != null && user.FirstName.Contains(searchTerm)) ||
                (user.LastName != null && user.LastName.Contains(searchTerm)) ||
                ((user.FirstName ?? string.Empty) + " " +
                 (user.LastName ?? string.Empty)).Contains(searchTerm) ||
                roleMatchedUserIds.Contains(user.Id));
        }

        var totalCount = await usersQuery.CountAsync();
        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(totalCount / (double)pageSize));

        if (page > totalPages)
        {
            page = totalPages;
        }

        // Load only the users needed for the current page.
        var users = await usersQuery
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Load role assignments for the current page in a single query.
        var rolesByUser = new Dictionary<string, List<string>>();
        var pageUserIds = users.Select(user => user.Id).ToList();

        if (pageUserIds.Count > 0)
        {
            var roleRows = await (
                from userRole in dbContext.Set<IdentityUserRole<string>>().AsNoTracking()
                join role in dbContext.Set<ApplicationRole>().AsNoTracking()
                    on userRole.RoleId equals role.Id
                where pageUserIds.Contains(userRole.UserId)
                orderby role.Name
                select new
                {
                    userRole.UserId,
                    RoleName = role.Name ?? string.Empty
                })
                .ToListAsync();

            foreach (var roleRow in roleRows)
            {
                if (!rolesByUser.TryGetValue(
                    roleRow.UserId,
                    out var roleNames))
                {
                    roleNames = new List<string>();
                    rolesByUser[roleRow.UserId] = roleNames;
                }

                if (!string.IsNullOrWhiteSpace(roleRow.RoleName))
                {
                    roleNames.Add(roleRow.RoleName);
                }
            }
        }

        var model = users.Select(user => new UserRoleListViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = rolesByUser.TryGetValue(user.Id, out var roleNames)
                ? roleNames
                : Enumerable.Empty<string>()
        }).ToList();

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.TotalPages = totalPages;
        ViewBag.StartItem = totalCount == 0
            ? 0
            : ((page - 1) * pageSize) + 1;
        ViewBag.EndItem = Math.Min(page * pageSize, totalCount);

        return View("~/Views/UserRoleManagement/Index.cshtml", model);
    }

    [HttpGet("Manage/{userId}")]
    public async Task<IActionResult> Manage(
        string userId,
        string? returnUrl = null)
    {
        ViewBag.userId = userId;

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ViewBag.ErrorMessage =
                $"User with Id = {userId} cannot be found";
            return View("NotFound");
        }

        ViewBag.UserName = user.UserName;
        ViewBag.ReturnUrl = GetSafeReturnUrl(returnUrl);

        var roles = await roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .ToListAsync();

        var userRoles = await userManager.GetRolesAsync(user);

        var model = roles.Select(role => new UserRoleEditViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name,
            Selected = role.Name != null && userRoles.Contains(role.Name)
        }).ToList();

        return View("~/Views/UserRoleManagement/Manage.cshtml", model);
    }

    [HttpPost("Manage/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Manage(
        List<UserRoleEditViewModel> model,
        string userId,
        string? returnUrl = null)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return View("NotFound");
        }

        ViewBag.userId = userId;
        ViewBag.UserName = user.UserName;
        ViewBag.ReturnUrl = GetSafeReturnUrl(returnUrl);

        var existingRoles = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(
            user,
            existingRoles);

        if (!removeResult.Succeeded)
        {
            ModelState.AddModelError(
                "",
                "Cannot remove user existing roles");

            return View(
                "~/Views/UserRoleManagement/Manage.cshtml",
                model);
        }

        var selectedRoles = model
            .Where(x => x.Selected && !string.IsNullOrWhiteSpace(x.RoleName))
            .Select(x => x.RoleName!);

        var addResult = await userManager.AddToRolesAsync(
            user,
            selectedRoles);

        if (!addResult.Succeeded)
        {
            ModelState.AddModelError(
                "",
                "Cannot add selected roles to user");

            return View(
                "~/Views/UserRoleManagement/Manage.cshtml",
                model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl)
            && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetSafeReturnUrl(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl)
            && Url.IsLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return "/Administrations/UserRoleManagement";
    }
}
