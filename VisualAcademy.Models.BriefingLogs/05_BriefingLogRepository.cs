using Dul.Articles;
using Dul.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Zero.Models
{
    /// <summary>
    /// [5] Repository Class: ADO.NET or Dapper or Entity Framework Core
    /// </summary>
    public class BriefingLogRepository : IBriefingLogRepository
    {
        private readonly BriefingLogAppDbContext _context;
        private readonly ILogger _logger;

        public BriefingLogRepository(BriefingLogAppDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(BriefingLogRepository));
        }

        //[6][1] 입력
        public async Task<BriefingLog> AddAsync(BriefingLog model)
        {
            try
            {
                _context.BriefingLogs.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
            }

            return model;
        }

        //[6][2] 출력
        public async Task<List<BriefingLog>> GetAllAsync()
        {
            return await _context.BriefingLogs.OrderByDescending(m => m.Id)
                .ToListAsync();
        }

        //[6][3] 상세
        public async Task<BriefingLog> GetByIdAsync(int id)
        {
            return await _context.BriefingLogs
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        //[6][4] 수정
        public async Task<bool> EditAsync(BriefingLog model)
        {
            try
            {
                _context.BriefingLogs.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR({nameof(EditAsync)}): {e.Message}");
            }

            return false;
        }

        //[6][5] 삭제
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var model = await _context.BriefingLogs.FindAsync(id);
                _context.Remove(model);
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR({nameof(DeleteAsync)}): {e.Message}");
            }

            return false;
        }

        //[6][6] 페이징
        public async Task<PagingResult<BriefingLog>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.BriefingLogs.CountAsync();
            var models = await _context.BriefingLogs
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<BriefingLog>(models, totalRecords);
        }

        // 부모
        public async Task<PagingResult<BriefingLog>> GetAllByParentIdAsync(int pageIndex, int pageSize, int parentId)
        {
            var totalRecords = await _context.BriefingLogs.Where(m => m.ParentId == parentId).CountAsync();
            var models = await _context.BriefingLogs
                .Where(m => m.ParentId == parentId)
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<BriefingLog>(models, totalRecords);
        }

        // 상태
        public async Task<Tuple<int, int>> GetStatus(int parentId)
        {
            var totalRecords = await _context.BriefingLogs.Where(m => m.ParentId == parentId).CountAsync();
            var pinnedRecords = await _context.BriefingLogs.Where(m => m.ParentId == parentId && m.IsPinned == true).CountAsync();

            return new Tuple<int, int>(pinnedRecords, totalRecords); // (2, 10)
        }

        // DeleteAllByParentId
        public async Task<bool> DeleteAllByParentId(int parentId)
        {
            try
            {
                var models = await _context.BriefingLogs.Where(m => m.ParentId == parentId).ToListAsync();

                foreach (var model in models)
                {
                    _context.BriefingLogs.Remove(model);
                }

                return (await _context.SaveChangesAsync() > 0 ? true : false);

            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR({nameof(DeleteAllByParentId)}): {e.Message}");
            }

            return false;
        }

        // 검색
        public async Task<PagingResult<BriefingLog>> SearchAllAsync(int pageIndex, int pageSize, string searchQuery)
        {
            var totalRecords = await _context.BriefingLogs
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.BriefingLogs
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<BriefingLog>(models, totalRecords);
        }

        public async Task<PagingResult<BriefingLog>> SearchAllByParentIdAsync(int pageIndex, int pageSize, string searchQuery, int parentId)
        {
            var totalRecords = await _context.BriefingLogs.Where(m => m.ParentId == parentId)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.BriefingLogs.Where(m => m.ParentId == parentId)
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<BriefingLog>(models, totalRecords);
        }

        public async Task<SortedList<int, double>> GetMonthlyCreateCountAsync()
        {
            SortedList<int, double> createCounts = new SortedList<int, double>();

            // 1월부터 12월까지 0.0으로 초기화
            for (int i = 1; i <= 12; i++)
            {
                createCounts[i] = 0.0;
            }

            for (int i = 0; i < 12; i++)
            {
                // 현재 달부터 12개월 전까지 반복
                var current = DateTime.Now.AddMonths(-i);
                var cnt = _context.BriefingLogs.AsEnumerable().Where(
                    m => m.Created != null
                    &&
                    Convert.ToDateTime(m.Created).Month == current.Month
                    &&
                    Convert.ToDateTime(m.Created).Year == current.Year
                ).ToList().Count();
                createCounts[current.Month] = cnt;
            }

            return await Task.FromResult(createCounts);
        }

        public async Task<PagingResult<BriefingLog>> GetAllByParentKeyAsync(int pageIndex, int pageSize, string parentKey)
        {
            var totalRecords = await _context.BriefingLogs.Where(m => m.ParentKey == parentKey).CountAsync();
            var models = await _context.BriefingLogs
                .Where(m => m.ParentKey == parentKey)
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<BriefingLog>(models, totalRecords);
        }

        public async Task<PagingResult<BriefingLog>> SearchAllByParentKeyAsync(int pageIndex, int pageSize, string searchQuery, string parentKey)
        {
            var totalRecords = await _context.BriefingLogs.Where(m => m.ParentKey == parentKey)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.BriefingLogs.Where(m => m.ParentKey == parentKey)
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<BriefingLog>(models, totalRecords);
        }

        public async Task<ArticleSet<BriefingLog, int>> GetArticles<TParentIdentifier>(int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier)
        {
            var items = _context.BriefingLogs.Select(m => m); // 메서드 구문(Method Syntax)

            // ParentBy 
            if (parentIdentifier is int parentId && parentId != 0)
            {
                items = items.Where(m => m.ParentId == parentId);
            }
            else if (parentIdentifier is string parentKey && !string.IsNullOrEmpty(parentKey))
            {
                items = items.Where(m => m.ParentKey == parentKey);
            }

            // Search Mode
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (searchField == "Name")
                {
                    // Name
                    items = items.Where(m => m.Name.Contains(searchQuery));
                }
                else if (searchField == "Title")
                {
                    // Title
                    items = items.Where(m => m.Title.Contains(searchQuery));
                }
                else
                {
                    // All
                    items = items.Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery));
                }
            }

            var totalCount = await items.CountAsync();

            // Sorting
            switch (sortOrder)
            {
                case "Name":
                    items = items.OrderBy(m => m.Name);
                    break;
                case "NameDesc":
                    items = items.OrderByDescending(m => m.Name);
                    break;
                case "Title":
                    items = items.OrderBy(m => m.Title);
                    break;
                case "TitleDesc":
                    items = items.OrderByDescending(m => m.Title);
                    break;
                default:
                    items = items.OrderByDescending(m => m.Id);
                    break;
            }

            // Paging
            items = items.Skip(pageIndex * pageSize).Take(pageSize);

            return new ArticleSet<BriefingLog, int>(await items.ToListAsync(), totalCount);
        }
    }
}
