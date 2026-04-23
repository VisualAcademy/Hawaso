#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Locations;

public class DetailsModel(Hawaso.Data.ApplicationDbContext context) : PageModel
{
    public Location? Location { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        Location = await context.Locations
            .Include(l => l.PropertyRef)
            .FirstOrDefaultAsync(m => m.Id == id.Value);

        if (Location is null)
        {
            return NotFound();
        }

        return Page();
    }
}