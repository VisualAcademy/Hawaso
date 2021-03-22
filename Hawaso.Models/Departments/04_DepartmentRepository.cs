using Dul.Articles;
using Dul.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Models
{
    public class DepartmentRepository : IDepartmentRepository, IDisposable
    {
        private readonly DepartmentAppDbContext _context;
        private readonly ILogger _logger;

        public DepartmentRepository(DepartmentAppDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(DepartmentRepository));
        }

        #region [4][1] 입력: AddAsync
        //[4][1] 입력: AddAsync
        public async Task<DepartmentModel> AddAsync(DepartmentModel model)
        {
            model.CreatedAt = DateTime.UtcNow;

            try
            {
                _context.Departments.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
            }

            return model;
        }
        #endregion

        #region [4][2] 출력: GetAllAsync
        //[4][2] 출력: GetAllAsync
        public async Task<List<DepartmentModel>> GetAllAsync()
        {
            return await _context.Departments.OrderByDescending(m => m.Id).ToListAsync();
        }
        #endregion

        #region [4][3] 상세: GetByIdAsync
        //[4][3] 상세: GetByIdAsync
        public async Task<DepartmentModel> GetByIdAsync(long id)
        {
            var model = await _context.Departments.SingleOrDefaultAsync(m => m.Id == id);

            return model;
        }
        #endregion

        #region [4][4] 수정: UpdateAsync
        //[4][4] 수정: UpdateAsync
        public async Task<bool> EditAsync(DepartmentModel model)
        {
            try
            {
                //_context.Departments.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(EditAsync)}): {e.Message}");
            }

            return false;
        }
        public async Task<bool> UpdateAsync(DepartmentModel model)
        {
            try
            {
                var m = _context.Departments.Find(model.Id);

                m.Name = model.Name;
                m.Active = model.Active;

                _context.Update(m);
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(UpdateAsync)}): {e.Message}");
            }

            return false;
        }
        #endregion

        #region [4][5] 삭제: DeleteAsync
        //[4][5] 삭제
        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var model = await _context.Departments.FindAsync(id);
                _context.Remove(model);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ಠ_ಠ) // Disapproval Look
            {
                _logger?.LogError($"ERROR({nameof(DeleteAsync)}): {ಠ_ಠ.Message}");
            }

            return false;
        }
        #endregion

        #region [4][6] 페이징: GetAllAsync()
        //[4][6] 페이징: GetAllAsync()
        public async Task<PagingResult<DepartmentModel>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Departments.CountAsync();
            var models = await _context.Departments
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<DepartmentModel>(models, totalRecords);
        } 
        #endregion

        //[4][7] 부모
        public async Task<PagingResult<DepartmentModel>> GetAllByParentIdAsync(
            int pageIndex,
            int pageSize,
            int parentId)
        {
            var totalRecords = await _context.Departments
                //.Where(m => m.ParentId == parentId)
                .CountAsync();
            var models = await _context.Departments
                //.Where(m => m.ParentId == parentId)
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<DepartmentModel>(models, totalRecords);
        }

        //[4][8] 상태
        public async Task<Tuple<int, int>> GetStatus(int parentId)
        {
            var totalRecords = await _context.Departments
                //.Where(m => m.ParentId == parentId)
                .CountAsync();
            var pinnedRecords = await _context.Departments
                //.Where(m => m.ParentId == parentId && m.IsPinned == true)
                .CountAsync();

            return new Tuple<int, int>(pinnedRecords, totalRecords); // (2, 10)
        }

        //[4][9] 부모 삭제
        public async Task<bool> DeleteAllByParentId(int parentId)
        {
            try
            {
                var models = await _context.Departments
                    //.Where(m => m.ParentId == parentId)
                    .ToListAsync();

                foreach (var model in models)
                {
                    _context.Departments.Remove(model);
                }

                return (await _context.SaveChangesAsync() > 0 ? true : false);

            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(DeleteAllByParentId)}): {e.Message}");
            }

            return false;
        }

        //[4][10] 검색
        public async Task<PagingResult<DepartmentModel>> SearchAllAsync(
            int pageIndex,
            int pageSize,
            string searchQuery)
        {
            var totalRecords = await _context.Departments
                .Where(m => m.Name.Contains(searchQuery) 
                //|| m.Title.Contains(searchQuery) 
                //|| m.Title.Contains(searchQuery)
                )
                .CountAsync();
            var models = await _context.Departments
                .Where(m => m.Name.Contains(searchQuery) 
                //|| m.Title.Contains(searchQuery) 
                //|| m.Title.Contains(searchQuery)
                )
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<DepartmentModel>(models, totalRecords);
        }

        //[4][11] 부모 검색
        public async Task<PagingResult<DepartmentModel>> SearchAllByParentIdAsync(
            int pageIndex,
            int pageSize,
            string searchQuery,
            int parentId)
        {
            var totalRecords = await _context.Departments
                //.Where(m => m.ParentId == parentId)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") 
                //|| m.Title.Contains(searchQuery) 
                )
                .CountAsync();
            var models = await _context.Departments
                //.Where(m => m.ParentId == parentId)
                .Where(m => m.Name.Contains(searchQuery) 
                //|| m.Title.Contains(searchQuery) 
                )
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<DepartmentModel>(models, totalRecords);
        }

        //[4][12] 통계
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
                var cnt = _context.Departments.AsEnumerable()
                    .Where(
                        m => m.CreatedAt != null
                        //&&
                        //Convert.ToDateTime(m.Created).Month == current.Month
                        //&&
                        //Convert.ToDateTime(m.Created).Year == current.Year
                    )
                    .ToList().Count();
                createCounts[current.Month] = cnt;
            }

            return await Task.FromResult(createCounts);
        }

        //[4][13] 부모 페이징
        public async Task<PagingResult<DepartmentModel>> GetAllByParentKeyAsync(
            int pageIndex,
            int pageSize,
            string parentKey)
        {
            var totalRecords = await _context.Departments
                //.Where(m => m.ParentKey == parentKey)
                .CountAsync();
            var models = await _context.Departments
                //.Where(m => m.ParentKey == parentKey)
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<DepartmentModel>(models, totalRecords);
        }

        //[4][14] 부모 검색
        public async Task<PagingResult<DepartmentModel>> SearchAllByParentKeyAsync(
            int pageIndex,
            int pageSize,
            string searchQuery,
            string parentKey)
        {
            var totalRecords = await _context.Departments
                //.Where(m => m.ParentKey == parentKey)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") 
                //|| m.Title.Contains(searchQuery) 
                )
                .CountAsync();
            var models = await _context.Departments
                //.Where(m => m.ParentKey == parentKey)
                .Where(m => m.Name.Contains(searchQuery) 
                //|| m.Title.Contains(searchQuery)
                )
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<DepartmentModel>(models, totalRecords);
        }

        //[4][15] 리스트(페이징, 검색, 정렬)
        public async Task<ArticleSet<DepartmentModel, int>> GetArticlesAsync<TParentIdentifier>(
            int pageIndex,
            int pageSize,
            string searchField,
            string searchQuery,
            string sortOrder,
            TParentIdentifier parentIdentifier)
        {
            var items = _context.Departments.AsQueryable();

            #region ParentBy: 특정 부모 키 값(int, string)에 해당하는 리스트인지 확인
            //// ParentBy 
            //if (parentIdentifier is int parentId && parentId != 0)
            //{
            //    items = items.Where(m => m.ParentId == parentId);
            //}
            //else if (parentIdentifier is string parentKey && !string.IsNullOrEmpty(parentKey))
            //{
            //    items = items.Where(m => m.ParentKey == parentKey);
            //}
            #endregion

            #region Search Mode: SearchField와 SearchQuery에 해당하는 데이터 검색
            // Search Mode
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (searchField == "Name")
                {
                    // Name
                    items = items
                        .Where(m => m.Name.Contains(searchQuery)).AsNoTracking();
                }
                //else if (searchField == "Title")
                //{
                //    // Title
                //    items = items;
                //        //.Where(m => m.Title.Contains(searchQuery));
                //}
                else
                {
                    // All: 기타 더 검색이 필요한 컬럼이 있다면 추가 가능
                    items = items
                        .Where(m => m.Name.Contains(searchQuery) 
                        //|| m.Title.Contains(searchQuery)
                        ).AsNoTracking();
                }
            } 
            #endregion

            // 총 레코드 수 계산
            var totalCount = await items.AsNoTracking().CountAsync();

            #region Sorting: 어떤 열에 대해 정렬(None, Asc, Desc)할 것인지 원하는 문자열로 지정
            // Sorting
            switch (sortOrder)
            {
                case "Name":
                    items = items
                        .OrderBy(m => m.Name).ThenByDescending(m => m.Id);
                    break;
                case "NameDesc":
                    items = items
                        .OrderByDescending(m => m.Name).ThenByDescending(m => m.Id);
                    break;
                default:
                    items = items
                        .OrderByDescending(m => m.Id).AsNoTracking(); // .ThenBy(m => m.RefOrder);
                    break;
            } 
            #endregion

            // Paging
            items = items.Skip(pageIndex * pageSize).Take(pageSize).AsNoTracking();

            return new ArticleSet<DepartmentModel, int>(await items.AsNoTracking().ToListAsync(), totalCount);
        }

        //[4][16] 답변: ReplyApp
        public async Task<DepartmentModel> AddAsync(DepartmentModel model, int parentRef, int parentStep, int parentRefOrder)
        {
            model.CreatedAt = DateTime.UtcNow;

            try
            {
                _context.Departments.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
            }

            return model;
        }

        //[4][17] 답변: DepartmentApp
        public async Task<DepartmentModel> AddAsync(DepartmentModel model, int parentId)
        {
            model.CreatedAt = DateTime.UtcNow;

            try
            {
                _context.Departments.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
            }

            return model;
        }

        #region [4][6] 검색: GetByAsync()
        //[4][6] 검색: GetByAsync()
        public async Task<ArticleSet<DepartmentModel, long>> GetByAsync<TParentIdentifier>(
            FilterOptions<TParentIdentifier> options)
        {
            var items = _context.Departments.AsQueryable();

            //#region ParentBy: 특정 부모 키 값(int, string)에 해당하는 리스트인지 확인
            //if (options.ChildMode)
            //{
            //    // ParentBy 
            //    if (options.ParentIdentifier is int parentId && parentId != 0)
            //    {
            //        //items = items.Where(m => m.ParentId == parentId);
            //    }
            //    else if (options.ParentIdentifier is string parentKey && !string.IsNullOrEmpty(parentKey))
            //    {
            //        //items = items.Where(m => m.ParentKey == parentKey);
            //    }
            //}
            //#endregion

            #region Search Mode: SearchField와 SearchQuery에 해당하는 데이터 검색
            if (options.SearchMode)
            {
                // Search Mode
                if (!string.IsNullOrEmpty(options.SearchQuery))
                {
                    var searchQuery = options.SearchQuery; // 검색어

                    if (options.SearchField == "Name")
                    {
                        // Name
                        items = items.Where(m => m.Name.Contains(searchQuery));
                    }
                    //else if (options.SearchField == "Title")
                    //{
                    //    // Title
                    //    items = items.Where(m => m.Title.Contains(searchQuery));
                    //}
                    //else if (options.SearchField == "Content")
                    //{
                    //    // Title
                    //    items = items.Where(m => m.Content.Contains(searchQuery));
                    //}
                    else
                    {
                        // All: 기타 더 검색이 필요한 컬럼이 있다면 추가 가능
                        items = items.Where(m => m.Name.Contains(searchQuery) 
                        //|| m.Title.Contains(searchQuery) || m.Content.Contains(searchQuery)
                        );
                    }
                }
            }
            #endregion

            // 총 레코드 수 계산
            var totalCount = await items.CountAsync();

            #region Sorting: 어떤 열에 대해 정렬(None, Asc, Desc)할 것인지 원하는 문자열로 지정
            if (options.SortMode)
            {
                // Sorting
                foreach (var sf in options.SortFields)
                {
                    switch ($"{sf.Key}{sf.Value}")
                    {
                        case "NameAsc":
                            //items = items.OrderBy(m => m.Name);
                            items = items.OrderBy(m => m.Name).ThenByDescending(m => m.Id);
                            break;
                        default:
                            //items = items.OrderByDescending(m => m.Id);
                            items = items.OrderByDescending(m => m.Id);
                            break;
                    }
                }
            }
            #endregion

            // Paging
            items = items.Skip(options.PageIndex * options.PageSize).Take(options.PageSize);

            return new ArticleSet<DepartmentModel, long>(await items.AsNoTracking().ToListAsync(), totalCount);
        }
        #endregion

        #region Dispose
        // https://docs.microsoft.com/ko-kr/dotnet/api/system.gc.suppressfinalize?view=net-5.0
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose(); //_context = null;
                }
            }
        }
        #endregion
    }
}
