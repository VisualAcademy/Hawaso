using Dul.Articles;
using Dul.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UploadApp.Models
{
    /// <summary>
    /// [6] Repository Class: ADO.NET or Dapper or Entity Framework Core
    /// </summary>
    public class UploadRepository : IUploadRepository
    {
        private readonly UploadAppDbContext _context;
        private readonly ILogger _logger;

        public UploadRepository(UploadAppDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(UploadRepository));
        }

        //[6][1] 입력
        public async Task<Upload> AddAsync(Upload model)
        {
            try
            {
                _context.Uploads.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
            }

            return model; 
        }

        //[6][2] 출력
        public async Task<List<Upload>> GetAllAsync()
        {
            return await _context.Uploads.OrderByDescending(m => m.Id)
                //.Include(m => m.UploadsComments)
                .ToListAsync(); 
        }

        //[6][3] 상세
        public async Task<Upload> GetByIdAsync(int id)
        {
            return await _context.Uploads
                //.Include(m => m.UploadsComments)
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        //[6][4] 수정
        public async Task<bool> EditAsync(Upload model)
        {
            try
            {
                _context.Uploads.Attach(model);
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
            //var model = await _context.Uploads.SingleOrDefaultAsync(m => m.Id == id);
            try
            {
                var model = await _context.Uploads.FindAsync(id);
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
        public async Task<PagingResult<Upload>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Uploads.CountAsync();
            var models = await _context.Uploads
                .OrderByDescending(m => m.Id)
                //.Include(m => m.UploadsComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Upload>(models, totalRecords); 
        }

        // 부모
        public async Task<PagingResult<Upload>> GetAllByParentIdAsync(int pageIndex, int pageSize, int parentId)
        {
            var totalRecords = await _context.Uploads.Where(m => m.ParentId == parentId).CountAsync();
            var models = await _context.Uploads
                .Where(m => m.ParentId == parentId)
                .OrderByDescending(m => m.Id)
                //.Include(m => m.UploadsComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Upload>(models, totalRecords);
        }

        // 상태
        public async Task<Tuple<int, int>> GetStatus(int parentId)
        {
            var totalRecords = await _context.Uploads.Where(m => m.ParentId == parentId).CountAsync();
            var pinnedRecords = await _context.Uploads.Where(m => m.ParentId == parentId && m.IsPinned == true).CountAsync();

            return new Tuple<int, int>(pinnedRecords, totalRecords); // (2, 10)
        }

        // DeleteAllByParentId
        public async Task<bool> DeleteAllByParentId(int parentId)
        {
            try
            {
                var models = await _context.Uploads.Where(m => m.ParentId == parentId).ToListAsync();

                foreach (var model in models)
                {
                    _context.Uploads.Remove(model);
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
        public async Task<PagingResult<Upload>> SearchAllAsync(int pageIndex, int pageSize, string searchQuery)
        {
            var totalRecords = await _context.Uploads
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Uploads
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                //.Include(m => m.UploadsComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Upload>(models, totalRecords);
        }

        public async Task<PagingResult<Upload>> SearchAllByParentIdAsync(int pageIndex, int pageSize, string searchQuery, int parentId)
        {
            var totalRecords = await _context.Uploads.Where(m => m.ParentId == parentId)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Uploads.Where(m => m.ParentId == parentId)
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                //.Include(m => m.UploadsComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Upload>(models, totalRecords);
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
                var cnt = _context.Uploads.AsEnumerable().Where(
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

        public async Task<PagingResult<Upload>> GetAllByParentKeyAsync(int pageIndex, int pageSize, string parentKey)
        {
            var totalRecords = await _context.Uploads.Where(m => m.ParentKey == parentKey).CountAsync();
            var models = await _context.Uploads
                .Where(m => m.ParentKey == parentKey)
                .OrderByDescending(m => m.Id)
                //.Include(m => m.UploadsComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Upload>(models, totalRecords);
        }

        public async Task<PagingResult<Upload>> SearchAllByParentKeyAsync(int pageIndex, int pageSize, string searchQuery, string parentKey)
        {
            var totalRecords = await _context.Uploads.Where(m => m.ParentKey == parentKey)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Uploads.Where(m => m.ParentKey == parentKey)
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                //.Include(m => m.UploadsComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Upload>(models, totalRecords);
        }

        public async Task<ArticleSet<Upload, int>> GetArticles<TParentIdentifier>(int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier) 
        {
            //var items = from m in _context.Uploads select m; // 쿼리 구문(Query Syntax)
            var items = _context.Uploads.Select(m => m); // 메서드 구문(Method Syntax)

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

            return new ArticleSet<Upload, int>(await items.ToListAsync(), totalCount);
        }
    }
}
