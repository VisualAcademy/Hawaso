#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Hawaso.Data;
using VisualAcademy.Models;

namespace VisualAcademy.Pages.Cascading.Properties;

public class CreateModel(Hawaso.Data.ApplicationDbContext context) : PageModel
{
    public IActionResult OnGet() => Page();

    [BindProperty]
    public Property Property { get; set; }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.Properties.Add(Property);
        await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
