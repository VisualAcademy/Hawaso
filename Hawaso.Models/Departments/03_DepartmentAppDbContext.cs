using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace Hawaso.Models
{
    public class DepartmentAppDbContext : DbContext
    {
        public DepartmentAppDbContext() : base() 
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DepartmentAppDbContext(DbContextOptions<DepartmentAppDbContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Departments 테이블의 Created, PostDate 열은 자동으로 GetDate() 제약 조건을 부여하기 
            modelBuilder.Entity<DepartmentModel>().Property(m => m.CreatedAt).HasDefaultValueSql("GetDate()");
        }

        //[!] DepartmentApp 솔루션 관련 모든 테이블에 대한 참조 
        public DbSet<DepartmentModel> Departments { get; set; }
    }
}
