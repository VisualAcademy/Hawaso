#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Locations
{
    public class IndexModel : PageModel
    {
        private readonly Hawaso.Data.ApplicationDbContext _context;

        public IndexModel(Hawaso.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Location> Location { get;set; }

        public async Task OnGetAsync()
        {
            Location = await _context.Locations
                .Include(l => l.PropertyRef).ToListAsync();
        }
    }
}
