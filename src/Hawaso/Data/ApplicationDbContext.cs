using VisualAcademy.Components.Pages.ApplicantsTransfers;
using VisualAcademy.Models;

namespace Hawaso.Data;

#region ASP.NET Core Identity 인증 확장
// ASP.NET Core Identity
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //public virtual DbSet<Department> Departments { get; set; }

    #region Cascading DropDownList 
    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<Sublocation> Sublocations { get; set; } = null!;
    #endregion

    public DbSet<Page> Pages { get; set; }

    public DbSet<ApplicantTransfer> ApplicantsTransfers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Page>().HasKey(p => p.Id);
        builder.Entity<Page>().Property(p => p.TenantName).HasDefaultValue("Hawaso");
    }
}

// 원본 모양 
//public class ApplicationDbContext : IdentityDbContext
//{
//    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//        : base(options)
//    {
//    }
//}
#endregion
