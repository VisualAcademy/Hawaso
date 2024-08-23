#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hawaso.Data;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Properties;

public class DetailsModel(ApplicationDbContext context) : PageModel
{
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
}
