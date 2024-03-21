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
