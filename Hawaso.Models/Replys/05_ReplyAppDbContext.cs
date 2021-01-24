using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace ReplyApp.Models
{
    /// <summary>
    /// [5] DbContext Class
    /// </summary>
    public class ReplyAppDbContext : DbContext
    {
        #region NuGet Packages
        // PM> Install-Package Microsoft.EntityFrameworkCore.SqlServer
        // PM> Install-Package Microsoft.Data.SqlClient
        // PM> Install-Package System.Configuration.ConfigurationManager
        // --OR--
        //// PM> Install-Package Microsoft.EntityFrameworkCore
        //// PM> Install-Package Microsoft.EntityFrameworkCore.Tools
        //// PM> Install-Package Microsoft.EntityFrameworkCore.InMemory 
        //// --OR--
        //// PM> Install-Package Microsoft.AspNetCore.All // 2.1 버전까지만 사용 가능 
        #endregion

        public ReplyAppDbContext()
        {
            // Empty
            // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public ReplyAppDbContext(DbContextOptions<ReplyAppDbContext> options)
            : base(options)
        {
            // 공식과 같은 코드, 교과서다운 코드
            // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <summary>
        /// 참고 코드: 닷넷 프레임워크 또는 Windows Forms/WPF 기반에서 호출되는 코드 영역
        /// __App.config 또는 Web.config의 연결 문자열 사용
        /// __직접 데이터베이스 연결문자열 설정 가능
        /// __.NET Core 또는 .NET 5 이상에서는 사용하지 않음
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
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
            modelBuilder.Entity<Reply>().Property(m => m.Created).HasDefaultValueSql("GetDate()");
        }

        //[!] ReplyApp 솔루션 관련 모든 테이블에 대한 참조 
        public DbSet<Reply> Replys { get; set; }
    }
}
