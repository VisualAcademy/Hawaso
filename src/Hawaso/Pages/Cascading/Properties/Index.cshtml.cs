#nullable disable
using Microsoft.AspNetCore.Mvc.RazorPages;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Properties
{
    public class IndexModel : PageModel
    {
        private readonly Hawaso.Data.ApplicationDbContext _context;

        public IndexModel(Hawaso.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Property> Property { get;set; }

        public async Task OnGetAsync()
        {
            Property = await _context.Properties.ToListAsync();
        }
    }
}
