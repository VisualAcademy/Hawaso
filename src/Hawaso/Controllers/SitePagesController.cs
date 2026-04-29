using Azunt.Web.Data;
using Azunt.Web.Models;
using Azunt.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Azunt.Web.Controllers;

[Authorize(Roles = "Administrators")]
public class SitePagesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly SitePageRouteSyncService _syncService;

    public SitePagesController(
        ApplicationDbContext context,
        SitePageRouteSyncService syncService)
    {
        _context = context;
        _syncService = syncService;
    }

    public async Task<IActionResult> Index()
    {
        var pages = await _context.SitePages
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.PageNumber)
            .ThenBy(x => x.RoutePattern)
            .ThenBy(x => x.HttpMethod)
            .ToListAsync();

        return View(pages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sync()
    {
        await _syncService.SyncAsync();

        TempData["Message"] = "Site pages have been synchronized from EndpointDataSource.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var page = await _context.SitePages.FindAsync(id);

        if (page == null)
        {
            return NotFound();
        }

        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, SitePage model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var page = await _context.SitePages.FindAsync(id);

        if (page == null)
        {
            return NotFound();
        }

        page.PageTitle = model.PageTitle;
        page.PageNumber = model.PageNumber;
        page.SortOrder = model.SortOrder;
        page.IsPublic = model.IsPublic;
        page.IsVisibleInDashboard = model.IsVisibleInDashboard;
        page.RequiredRoles = model.RequiredRoles;
        page.RequiredPolicy = model.RequiredPolicy;
        page.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Message"] = "Site page has been updated.";

        return RedirectToAction(nameof(Index));
    }
}