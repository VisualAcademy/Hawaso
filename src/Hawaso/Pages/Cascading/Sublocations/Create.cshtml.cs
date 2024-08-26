#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hawaso.Data;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Sublocations;

public class CreateModel(ApplicationDbContext context) : PageModel
{
    public IActionResult OnGet()
    {
        ViewData["LocationId"] = new SelectList(context.Locations, "Id", "Name");
        return Page();
    }

    [BindProperty]
    public Sublocation Sublocation { get; set; }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.Sublocations.Add(Sublocation);
        await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
