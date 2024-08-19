#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Locations;

public class DetailsModel(Hawaso.Data.ApplicationDbContext context) : PageModel
{
    public Location Location { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Location = await context.Locations
            .Include(l => l.PropertyRef).FirstOrDefaultAsync(m => m.Id == id);

        if (Location == null)
        {
            return NotFound();
        }
        return Page();
    }
}
