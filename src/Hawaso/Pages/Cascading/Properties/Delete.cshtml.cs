#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hawaso.Data;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Properties;

public class DeleteModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public Property Property { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Property = await context.Properties.FirstOrDefaultAsync(m => m.Id == id);

        if (Property == null)
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

        Property = await context.Properties.FindAsync(id);

        if (Property != null)
        {
            context.Properties.Remove(Property);
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
