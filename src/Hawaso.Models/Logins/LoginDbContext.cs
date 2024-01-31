using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace Hawaso.Models
{
    /// <summary>
    /// LoginDbContext 클래스: 데이터베이스와 일대일로 매핑되는 컨텍스트 클래스 
    /// </summary>
    public class LoginDbContext : DbContext
    {
        // PM> Install-Package Microsoft.EntityFrameworkCore
        // PM> Install-Package Microsoft.EntityFrameworkCore.SqlServer
        // PM> Install-Package Microsoft.EntityFrameworkCore.Tools
        // PM> Install-Package Microsoft.EntityFrameworkCore.InMemory
        // PM> Install-Package System.Configuration.ConfigurationManager
        // PM> Install-Package Microsoft.Data.SqlClient

        public LoginDbContext()
        {
            // Empty
            // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public LoginDbContext(DbContextOptions<LoginDbContext> options)
            : base(options)
        {
            // 공식과 같은 코드
            // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 닷넷 프레임워크 기반에서 호출되는 코드 영역: 
            // App.config 또는 Web.config의 연결 문자열 사용
            // 직접 데이터베이스 연결문자열 설정 가능
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = ConfigurationManager.ConnectionStrings[
                    "ConnectionString"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Replys 테이블의 Created 열은 자동으로 GetDate() 제약 조건을 부여하기 
            modelBuilder.Entity<Login>().Property(m => m.LoginDate).HasDefaultValueSql("GetDate()");
        }

        /// <summary>
        /// Logins 속성: Logins 테이블과 일대일 
        /// </summary>
        public DbSet<Login> Logins { get; set; }
    }
}
