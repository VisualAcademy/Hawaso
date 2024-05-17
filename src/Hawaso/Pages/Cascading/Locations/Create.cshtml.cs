#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Locations
{
    public class CreateModel(Hawaso.Data.ApplicationDbContext context) : PageModel
    {
        public IActionResult OnGet()
        {
            ViewData["PropertyId"] = new SelectList(context.Properties, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Location Location { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            context.Locations.Add(Location);
            await context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
