#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Locations;

public class DeleteModel : PageModel
{
    private readonly Hawaso.Data.ApplicationDbContext _context;

    public DeleteModel(Hawaso.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Location Location { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Location = await _context.Locations
            .Include(l => l.PropertyRef).FirstOrDefaultAsync(m => m.Id == id);

        if (Location == null)
        {
            return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Location = await _context.Locations.FindAsync(id);

        if (Location != null)
        {
            _context.Locations.Remove(Location);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
