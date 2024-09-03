#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hawaso.Data;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Sublocations;

public class IndexModel(ApplicationDbContext context) : PageModel
{
    public IList<Sublocation> Sublocation { get;set; }

    public async Task OnGetAsync()
    {
        Sublocation = await context.Sublocations
            .Include(s => s.LocationRef).ToListAsync();
    }
}
