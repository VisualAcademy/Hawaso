#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hawaso.Data;
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
