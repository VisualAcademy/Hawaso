﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hawaso.Data;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Sublocations
{
    public class DetailsModel : PageModel
    {
        private readonly Hawaso.Data.ApplicationDbContext _context;

        public DetailsModel(Hawaso.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Sublocation Sublocation { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sublocation = await _context.Sublocations
                .Include(s => s.LocationRef).FirstOrDefaultAsync(m => m.Id == id);

            if (Sublocation == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
