using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Zero.Data;
using Zero.Models;

namespace Zero.Controllers
{
    public class CaseStatusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaseStatusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CaseStatus
        public async Task<IActionResult> Index()
        {
            return View(await _context.CaseStatuses.ToListAsync());
        }

        // GET: CaseStatus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caseStatus = await _context.CaseStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caseStatus == null)
            {
                return NotFound();
            }

            return View(caseStatus);
        }

        // GET: CaseStatus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CaseStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CaseStatusName,Active")] CaseStatus caseStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(caseStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(caseStatus);
        }

        // GET: CaseStatus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caseStatus = await _context.CaseStatuses.FindAsync(id);
            if (caseStatus == null)
            {
                return NotFound();
            }
            return View(caseStatus);
        }

        // POST: CaseStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CaseStatusName,Active")] CaseStatus caseStatus)
        {
            if (id != caseStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(caseStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaseStatusExists(caseStatus.Id))
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
            return View(caseStatus);
        }

        // GET: CaseStatus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caseStatus = await _context.CaseStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caseStatus == null)
            {
                return NotFound();
            }

            return View(caseStatus);
        }

        // POST: CaseStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var caseStatus = await _context.CaseStatuses.FindAsync(id);
            _context.CaseStatuses.Remove(caseStatus);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CaseStatusExists(int id)
        {
            return _context.CaseStatuses.Any(e => e.Id == id);
        }
    }
}
