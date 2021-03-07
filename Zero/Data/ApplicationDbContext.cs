using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zero.Models;

namespace Zero.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ReportType> ReportTypes { get; set; }
        public virtual DbSet<CaseStatus> CaseStatuses { get; set; }
        public virtual DbSet<ReportSpecific> ReportSpecifics { get; set; }
        public virtual DbSet<Sublocation> Sublocations { get; set; }
        public virtual DbSet<Incident> Incidents { get; set; }
    }
}
