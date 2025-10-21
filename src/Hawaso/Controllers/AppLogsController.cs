using Azunt.Data;
using Azunt.Models;
using Azunt.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Azunt.Controllers
{
    public class AppLogsController : Controller
    {
        private readonly LogsDbContext _db;
        public AppLogsController(LogsDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, string? level, int page = 1, int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 200);

            var query = _db.AppLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(x => x.Level == level);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    (x.Message != null && x.Message.Contains(q)) ||
                    (x.Exception != null && x.Exception.Contains(q)) ||
                    (x.Properties != null && x.Properties.Contains(q)) ||
                    (x.Level != null && x.Level.Contains(q)));
            }

            query = query.OrderByDescending(x => x.TimeStamp);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = items.Select(x => new LogListItem
            {
                TimeStamp = x.TimeStamp,
                Level = x.Level,
                MessageShort = Trim(x.Message, 120),
                Key = LogKey.MakeKey(x)
            }).ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.Query = q;
            ViewBag.Level = level;

            return View(vm);

            static string Trim(string? s, int n)
                => string.IsNullOrEmpty(s) ? "" : (s.Length > n ? s[..n] + "…" : s);
        }

        // GET: /AppLogs/Details?key=...&ts=...&level=...
        public async Task<IActionResult> Details(string key, string ts, string? level)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(ts))
                return BadRequest();

            if (!DateTimeOffset.TryParseExact(ts, "o", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var timestamp))
                return BadRequest("Invalid timestamp.");

            var candidates = _db.AppLogs.AsNoTracking().Where(x => x.TimeStamp == timestamp);
            if (!string.IsNullOrWhiteSpace(level))
                candidates = candidates.Where(x => x.Level == level);

            var list = await candidates.ToListAsync();
            var match = list.FirstOrDefault(x => LogKey.MakeKey(x) == key);
            if (match == null) return NotFound();

            return View(match);
        }
    }

    public sealed class LogListItem
    {
        public string? Level { get; set; }
        public DateTimeOffset? TimeStamp { get; set; }
        public string? MessageShort { get; set; }
        public string Key { get; set; } = "";
    }
}
