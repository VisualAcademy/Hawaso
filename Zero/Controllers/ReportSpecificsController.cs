using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Zero.Data;
using Zero.Models;

namespace Zero.Controllers
{
    public class ReportSpecificsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportSpecificsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReportSpecifics
        public async Task<IActionResult> Index()
        {
            var zeroContext = _context.ReportSpecifics.Include(r => r.ReportType);
            return View(await zeroContext.ToListAsync());
        }

        // GET: ReportSpecifics/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportSpecific = await _context.ReportSpecifics
                .Include(r => r.ReportType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reportSpecific == null)
            {
                return NotFound();
            }

            return View(reportSpecific);
        }

        // GET: ReportSpecifics/Create
        public IActionResult Create()
        {
            ViewData["ReportTypeId"] = new SelectList(_context.ReportTypes, "Id", "Id");
            return View();
        }

        // POST: ReportSpecifics/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Specific,Active,ReportTypeId")] ReportSpecific reportSpecific)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reportSpecific);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReportTypeId"] = new SelectList(_context.ReportTypes, "Id", "Id", reportSpecific.ReportTypeId);
            return View(reportSpecific);
        }

        // GET: ReportSpecifics/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportSpecific = await _context.ReportSpecifics.FindAsync(id);
            if (reportSpecific == null)
            {
                return NotFound();
            }
            ViewData["ReportTypeId"] = new SelectList(_context.ReportTypes, "Id", "Id", reportSpecific.ReportTypeId);
            return View(reportSpecific);
        }

        // POST: ReportSpecifics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Specific,Active,ReportTypeId")] ReportSpecific reportSpecific)
        {
            if (id != reportSpecific.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reportSpecific);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportSpecificExists(reportSpecific.Id))
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
            ViewData["ReportTypeId"] = new SelectList(_context.ReportTypes, "Id", "Id", reportSpecific.ReportTypeId);
            return View(reportSpecific);
        }

        // GET: ReportSpecifics/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportSpecific = await _context.ReportSpecifics
                .Include(r => r.ReportType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reportSpecific == null)
            {
                return NotFound();
            }

            return View(reportSpecific);
        }

        // POST: ReportSpecifics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reportSpecific = await _context.ReportSpecifics.FindAsync(id);
            _context.ReportSpecifics.Remove(reportSpecific);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportSpecificExists(int id)
        {
            return _context.ReportSpecifics.Any(e => e.Id == id);
        }
    }
}
