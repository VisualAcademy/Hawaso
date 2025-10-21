using Azunt.Models;
using Microsoft.EntityFrameworkCore;

namespace Azunt.Data
{
    public class LogsDbContext : DbContext
    {
        public LogsDbContext(DbContextOptions<LogsDbContext> options) : base(options) { }

        public DbSet<AppLog> AppLogs => Set<AppLog>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<AppLog>(e =>
            {
                e.ToTable("AppLogs"); // dbo.AppLogs
                e.HasNoKey();         // No Id key in schema
                e.HasIndex(nameof(AppLog.TimeStamp));
                e.HasIndex(nameof(AppLog.Level));
            });
        }
    }
}
