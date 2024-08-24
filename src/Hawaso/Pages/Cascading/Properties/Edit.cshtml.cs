#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hawaso.Data;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Properties;

public class EditModel(ApplicationDbContext context) : PageModel
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

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.Attach(Property).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PropertyExists(Property.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool PropertyExists(int id)
    {
        return context.Properties.Any(e => e.Id == id);
    }
}
