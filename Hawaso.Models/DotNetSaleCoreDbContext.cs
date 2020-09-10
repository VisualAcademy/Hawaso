using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace DotNetSaleCore.Models
{
    /// <summary>
    /// [5] DbContext Class
    /// </summary>
    public class DotNetSaleCoreDbContext : DbContext
    {
        // Install-Package Microsoft.EntityFrameworkCore.SqlServer
        // Install-Package Microsoft.EntityFrameworkCore.InMemory
        // Install-Package System.Configuration.ConfigurationManager

        public DotNetSaleCoreDbContext()
        {
            // Empty
        }

        public DotNetSaleCoreDbContext(DbContextOptions<DotNetSaleCoreDbContext> options)
            : base(options)
        {
            // 공식과 같은 코드
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 닷넷 프레임워크 기반에서 호출되는 코드 영역: 
            // App.config 또는 Web.config의 연결 문자열 사용
            // 직접 데이터베이스 연결문자열 설정 가능
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customers 테이블의 Created 열은 자동으로 GetDate() 제약 조건을 부여하기 
            modelBuilder.Entity<Category>().HasMany(c => c.Products).WithOne(a => a.Category).HasForeignKey(a => a.CategoryId);
            modelBuilder.Entity<Customer>().Property(m => m.Created).HasDefaultValueSql("GetDate()");
            modelBuilder.Entity<Product>().Property(m => m.RegistDate).HasDefaultValueSql("GetDate()");
        }

        //[!] DotNetSaleCore 솔루션 관련 모든 테이블에 대한 참조
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        //public DbSet<Order> Orders { get; set; }
    }
}
