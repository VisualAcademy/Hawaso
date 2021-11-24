//Install-Package Microsoft.EntityFrameworkCore
//Install-Package Microsoft.EntityFrameworkCore.SqlServer
//Install-Package Microsoft.EntityFrameworkCore.Tools
//Install-Package System.Configuration.ConfigurationManager
//Install-Package Microsoft.Data.SqlClient

using Hawaso.Models.Notes;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace DotNetNote.Models
{
    public class NoteDbContext : DbContext
    {
        public NoteDbContext()
        {
            // Empty
        }

        public NoteDbContext(DbContextOptions<NoteDbContext> options)
            : base(options)
        {
            // 공식과 같은 코드 
        }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            // 닷넷 프레임워크 기반에서 호출되는 코드 영역: 
            // App.Config 또는 Web.Config의 연결 문자열 사용
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = ConfigurationManager.ConnectionStrings[
                    "ConnectionString"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        /// <summary>
        /// 게시판
        /// </summary>
        public DbSet<Note> Notes { get; set; }

        /// <summary>
        /// 게시판 댓글
        /// </summary>
        public DbSet<NoteComment> NoteComments { get; set; }
    }
}
