﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Hawaso.Data;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Sublocations;

public class CreateModel : PageModel
{
    private readonly Hawaso.Data.ApplicationDbContext _context;

    public CreateModel(Hawaso.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult OnGet()
    {
        ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
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

        _context.Sublocations.Add(Sublocation);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
