#nullable disable
using Hawaso.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Sublocations;

public class DeleteModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public Sublocation Sublocation { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var sublocation = await context.Sublocations
            .Include(s => s.LocationRef)
            .FirstOrDefaultAsync(m => m.Id == id.Value);

        if (sublocation is null)
        {
            return NotFound();
        }

        Sublocation = sublocation;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var sublocation = await context.Sublocations.FindAsync(id.Value);

        if (sublocation is not null)
        {
            context.Sublocations.Remove(sublocation);
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}