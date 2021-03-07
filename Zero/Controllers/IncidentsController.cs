using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Zero.Data;
using Zero.Models;

namespace Zero.Controllers
{
    public class IncidentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncidentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Incidents
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Incidents.Include(i => i.CaseStatuses).Include(i => i.Departments).Include(i => i.Locations).Include(i => i.Properties).Include(i => i.ReportSpecific).Include(i => i.ReportTypes).Include(i => i.Sublocations);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Incidents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incident = await _context.Incidents
                .Include(i => i.CaseStatuses)
                .Include(i => i.Departments)
                .Include(i => i.Locations)
                .Include(i => i.Properties)
                .Include(i => i.ReportSpecific)
                .Include(i => i.ReportTypes)
                .Include(i => i.Sublocations)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (incident == null)
            {
                return NotFound();
            }

            return View(incident);
        }

        // GET: Incidents/Create
        public IActionResult Create()
        {
            ViewData["CaseStatusId"] = new SelectList(_context.CaseStatuses, "Id", "CaseStatusName");
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Dept");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["PropertyId"] = new SelectList(_context.Properties, "Id", "Name");
            ViewData["ReportSpecificId"] = new SelectList(_context.ReportSpecifics, "Id", "Id");
            ViewData["ReportTypeId"] = new SelectList(_context.ReportTypes, "Id", "Id");
            ViewData["SublocationId"] = new SelectList(_context.Sublocations, "Id", "Id");
            return View();
        }

        // POST: Incidents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CaseNumber,DailyLogId,DailyLogNumber,OpenedDate,Occurred,Closed,PropertyId,Property,ReportTypeId,ReportSpecificId,Specific,SpecificId,CaseStatusId,LocationId,Location,SublocationId,Sublocation,DepartmentId,Department,Summary,Details,Resolution,Reference,SecondaryOperatorId,SecondaryOperator,Custodial,UseForce,Medical,RiskManagement,Active,Priority,AgentName,SupervisorName,ManagerName,DirectorName,AgentSignature,SupervisorSignature,ManagerSignature,DirectorSignature,AgentTime,SupervisorTime,ManagerTime,DirectorTime,CaseTypeId,InvestigatorId,ShiftId,ImmediateSupervisorId,GamingClassId,SurveillanceNotified,SurveillanceObserverId,InitialContact,InspectorSig,DeputyDirectorSig,DirectorSig,SupervisorSig,CompletionDate,TgaforwardDate,TgoreturnDate,Citation,ViolationNature,Variance,Employee,ManagerId,TapeIdentification,ActionTaken,SuspectPhoto,ExclusionInfo,Notification,PoliceContacted,PoliceContact,InvestigatorSigTs,SupervisorSigTs,DeputyDirectorSigTs,DirectorSigTs,Tgoresponse,CreatedBy,ClosedBy,ModifiedBy,ModifiedDate,DispatchCallId,AuditId,SavingsOrLosses,DirectorOnly,CaseType,Status,Agent,AgentSigFile,SupervisorSigFile,ManagerSigFile,DirectorSigFile,AgentImage,SupervisorImage,ManagerImage,DirectorImage,RemarksTitle1,RemarksMemos1,RemarksTitle2,RemarksMemos2,RemarksTitle3,RemarksMemos3,RemarksTitle4,RemarksMemos4,RemarksTitle5,RemarksMemos5")] Incident incident)
        {
            if (ModelState.IsValid)
            {
                _context.Add(incident);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CaseStatusId"] = new SelectList(_context.CaseStatuses, "Id", "Id", incident.CaseStatusId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", incident.DepartmentId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Id", incident.LocationId);
            ViewData["PropertyId"] = new SelectList(_context.Properties, "Id", "Id", incident.PropertyId);
            ViewData["ReportSpecificId"] = new SelectList(_context.ReportSpecifics, "Id", "Specific", incident.ReportSpecificId);
            ViewData["ReportTypeId"] = new SelectList(_context.ReportTypes, "Id", "TypeName", incident.ReportTypeId);
            ViewData["SublocationId"] = new SelectList(_context.Sublocations, "Id", "SublocationName", incident.SublocationId);
            return View(incident);
        }

        // GET: Incidents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null)
            {
                return NotFound();
            }
            ViewData["CaseStatusId"] = new SelectList(_context.CaseStatuses, "Id", "CaseStatusName", incident.CaseStatusId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Dept", incident.DepartmentId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", incident.LocationId);
            ViewData["PropertyId"] = new SelectList(_context.Properties, "Id", "Name", incident.PropertyId);
            ViewData["ReportSpecificId"] = new SelectList(_context.ReportSpecifics, "Id", "Specific", incident.ReportSpecificId);
            ViewData["ReportTypeId"] = new SelectList(_context.ReportTypes, "Id", "TypeName", incident.ReportTypeId);
            ViewData["SublocationId"] = new SelectList(_context.Sublocations, "Id", "SublocationName", incident.SublocationId);
            return View(incident);
        }

        // POST: Incidents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CaseNumber,DailyLogId,DailyLogNumber,OpenedDate,Occurred,Closed,PropertyId,Property,ReportTypeId,ReportSpecificId,Specific,SpecificId,CaseStatusId,LocationId,Location,SublocationId,Sublocation,DepartmentId,Department,Summary,Details,Resolution,Reference,SecondaryOperatorId,SecondaryOperator,Custodial,UseForce,Medical,RiskManagement,Active,Priority,AgentName,SupervisorName,ManagerName,DirectorName,AgentSignature,SupervisorSignature,ManagerSignature,DirectorSignature,AgentTime,SupervisorTime,ManagerTime,DirectorTime,CaseTypeId,InvestigatorId,ShiftId,ImmediateSupervisorId,GamingClassId,SurveillanceNotified,SurveillanceObserverId,InitialContact,InspectorSig,DeputyDirectorSig,DirectorSig,SupervisorSig,CompletionDate,TgaforwardDate,TgoreturnDate,Citation,ViolationNature,Variance,Employee,ManagerId,TapeIdentification,ActionTaken,SuspectPhoto,ExclusionInfo,Notification,PoliceContacted,PoliceContact,InvestigatorSigTs,SupervisorSigTs,DeputyDirectorSigTs,DirectorSigTs,Tgoresponse,CreatedBy,ClosedBy,ModifiedBy,ModifiedDate,DispatchCallId,AuditId,SavingsOrLosses,DirectorOnly,CaseType,Status,Agent,AgentSigFile,SupervisorSigFile,ManagerSigFile,DirectorSigFile,AgentImage,SupervisorImage,ManagerImage,DirectorImage,RemarksTitle1,RemarksMemos1,RemarksTitle2,RemarksMemos2,RemarksTitle3,RemarksMemos3,RemarksTitle4,RemarksMemos4,RemarksTitle5,RemarksMemos5")] Incident incident)
        {
            if (id != incident.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(incident);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncidentExists(incident.Id))
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
            ViewData["CaseStatusId"] = new SelectList(_context.CaseStatuses, "Id", "Id", incident.CaseStatusId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", incident.DepartmentId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Id", incident.LocationId);
            ViewData["PropertyId"] = new SelectList(_context.Properties, "Id", "Id", incident.PropertyId);
            ViewData["ReportSpecificId"] = new SelectList(_context.ReportSpecifics, "Id", "Id", incident.ReportSpecificId);
            ViewData["ReportTypeId"] = new SelectList(_context.ReportTypes, "Id", "Id", incident.ReportTypeId);
            ViewData["SublocationId"] = new SelectList(_context.Sublocations, "Id", "Id", incident.SublocationId);
            return View(incident);
        }

        // GET: Incidents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incident = await _context.Incidents
                .Include(i => i.CaseStatuses)
                .Include(i => i.Departments)
                .Include(i => i.Locations)
                .Include(i => i.Properties)
                .Include(i => i.ReportSpecific)
                .Include(i => i.ReportTypes)
                .Include(i => i.Sublocations)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (incident == null)
            {
                return NotFound();
            }

            return View(incident);
        }

        // POST: Incidents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            _context.Incidents.Remove(incident);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IncidentExists(int id)
        {
            return _context.Incidents.Any(e => e.Id == id);
        }
    }
}
