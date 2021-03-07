using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Zero.Data;
using Zero.Models;

namespace Zero.Controllers
{
    public class SublocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SublocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sublocations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sublocations.ToListAsync());
        }

        // GET: Sublocations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sublocation = await _context.Sublocations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sublocation == null)
            {
                return NotFound();
            }

            return View(sublocation);
        }

        // GET: Sublocations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sublocations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SublocationName,Active,Location,Property")] Sublocation sublocation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sublocation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sublocation);
        }

        // GET: Sublocations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sublocation = await _context.Sublocations.FindAsync(id);
            if (sublocation == null)
            {
                return NotFound();
            }
            return View(sublocation);
        }

        // POST: Sublocations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SublocationName,Active,Location,Property")] Sublocation sublocation)
        {
            if (id != sublocation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sublocation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SublocationExists(sublocation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sublocation);
        }

        // GET: Sublocations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sublocation = await _context.Sublocations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sublocation == null)
            {
                return NotFound();
            }

            return View(sublocation);
        }

        // POST: Sublocations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sublocation = await _context.Sublocations.FindAsync(id);
            _context.Sublocations.Remove(sublocation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SublocationExists(int id)
        {
            return _context.Sublocations.Any(e => e.Id == id);
        }
    }
}
