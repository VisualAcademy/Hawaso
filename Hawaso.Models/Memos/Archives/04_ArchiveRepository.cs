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
    /// <summary>
    /// [4] Repository Class: ADO.NET or Dapper(Micro ORM) or Entity Framework Core(ORM)
    /// ~Repository, ~Provider, ~Data
    /// </summary>
    public class ArchiveRepository : IArchiveRepository, IDisposable
    {
        private readonly ArchiveAppDbContext _context;
        private readonly ILogger _logger;

        public ArchiveRepository(ArchiveAppDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(ArchiveRepository));
        }

        #region [4][1] 입력: AddAsync
        //[4][1] 입력: AddAsync
        public async Task<Archive> AddAsync(Archive model)
        {
            #region 답변 기능 추가
            // 현재테이블의 Ref의 Max값 가져오기
            int maxRef = 1;
            int? max = await _context.Archives.DefaultIfEmpty().MaxAsync(m => m == null ? 0 : m.Ref);
            if (max.HasValue)
            {
                maxRef = (int)max + 1;
            }

            model.Ref = maxRef; // 참조 글(부모 글, 그룹 번호)
            model.Step = 0; // 들여쓰기(처음 글을 0으로 초기화)
            model.RefOrder = 0; // 참조(그룹) 순서
            #endregion

            model.Created = DateTime.UtcNow;

            try
            {
                _context.Archives.Add(model);
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
        public async Task<List<Archive>> GetAllAsync()
        {
            return await _context.Archives.OrderByDescending(m => m.Id)
                .ToListAsync();
        }
        #endregion

        #region [4][3] 상세: GetByIdAsync
        //[4][3] 상세: GetByIdAsync
        public async Task<Archive> GetByIdAsync(long id)
        {
            var model = await _context.Archives
                .SingleOrDefaultAsync(m => m.Id == id);

            // ReadCount++
            if (model != null)
            {
                if (model.ReadCount != null)
                {
                    model.ReadCount = model.ReadCount + 1;
                }
                else
                {
                    model.ReadCount = 1; 
                }
                _context.Archives.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                _context.SaveChanges();
            }

            return model;
        }
        #endregion

        #region [4][4] 수정: UpdateAsync
        //[4][4] 수정: UpdateAsync
        public async Task<bool> EditAsync(Archive model)
        {
            try
            {
                model.ModifyDate = DateTime.Now;

                _context.Archives.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(EditAsync)}): {e.Message}");
            }

            return false;
        }
        public async Task<bool> UpdateAsync(Archive model)
        {
            try
            {
                var old = _context.Archives.Find(model.Id);

                old.Name = model.Name;
                old.Email = model.Email;
                old.Homepage = model.Homepage;
                old.Title = model.Title;
                old.Content = model.Content;
                old.IsPinned = model.IsPinned;
                old.Encoding = model.Encoding; 

                // TODO: 더 넣을 항목 처리: 이 부분은 어떻게 처리하는게 가장 좋은지 고민

                old.Modified = DateTimeOffset.UtcNow;

                _context.Update(old);
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
                var model = await _context.Archives.FindAsync(id);
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
        public async Task<PagingResult<Archive>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Archives.CountAsync();
            var models = await _context.Archives
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Archive>(models, totalRecords);
        } 
        #endregion

        //[4][7] 부모
        public async Task<PagingResult<Archive>> GetAllByParentIdAsync(
            int pageIndex,
            int pageSize,
            int parentId)
        {
            var totalRecords = await _context.Archives
                .Where(m => m.ParentId == parentId)
                .CountAsync();
            var models = await _context.Archives
                .Where(m => m.ParentId == parentId)
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Archive>(models, totalRecords);
        }

        //[4][8] 상태
        public async Task<Tuple<int, int>> GetStatus(int parentId)
        {
            var totalRecords = await _context.Archives
                .Where(m => m.ParentId == parentId)
                .CountAsync();
            var pinnedRecords = await _context.Archives
                .Where(m => m.ParentId == parentId && m.IsPinned == true)
                .CountAsync();

            return new Tuple<int, int>(pinnedRecords, totalRecords); // (2, 10)
        }

        //[4][9] 부모 삭제
        public async Task<bool> DeleteAllByParentId(int parentId)
        {
            try
            {
                var models = await _context.Archives
                    .Where(m => m.ParentId == parentId)
                    .ToListAsync();

                foreach (var model in models)
                {
                    _context.Archives.Remove(model);
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
        public async Task<PagingResult<Archive>> SearchAllAsync(
            int pageIndex,
            int pageSize,
            string searchQuery)
        {
            var totalRecords = await _context.Archives
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Archives
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Archive>(models, totalRecords);
        }

        //[4][11] 부모 검색
        public async Task<PagingResult<Archive>> SearchAllByParentIdAsync(
            int pageIndex,
            int pageSize,
            string searchQuery,
            int parentId)
        {
            var totalRecords = await _context.Archives
                .Where(m => m.ParentId == parentId)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Archives
                .Where(m => m.ParentId == parentId)
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Archive>(models, totalRecords);
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
                var cnt = _context.Archives.AsEnumerable()
                    .Where(
                        m => m.Created != null
                        &&
                        Convert.ToDateTime(m.Created).Month == current.Month
                        &&
                        Convert.ToDateTime(m.Created).Year == current.Year
                    )
                    .ToList().Count();
                createCounts[current.Month] = cnt;
            }

            return await Task.FromResult(createCounts);
        }

        //[4][13] 부모 페이징
        public async Task<PagingResult<Archive>> GetAllByParentKeyAsync(
            int pageIndex,
            int pageSize,
            string parentKey)
        {
            var totalRecords = await _context.Archives
                .Where(m => m.ParentKey == parentKey)
                .CountAsync();
            var models = await _context.Archives
                .Where(m => m.ParentKey == parentKey)
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Archive>(models, totalRecords);
        }

        //[4][14] 부모 검색
        public async Task<PagingResult<Archive>> SearchAllByParentKeyAsync(
            int pageIndex,
            int pageSize,
            string searchQuery,
            string parentKey)
        {
            var totalRecords = await _context.Archives
                .Where(m => m.ParentKey == parentKey)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Archives
                .Where(m => m.ParentKey == parentKey)
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Title.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Archive>(models, totalRecords);
        }

        //[4][15] 리스트(페이징, 검색, 정렬)
        public async Task<ArticleSet<Archive, int>> GetArticlesAsync<TParentIdentifier>(
            int pageIndex,
            int pageSize,
            string searchField,
            string searchQuery,
            string sortOrder,
            TParentIdentifier parentIdentifier)
        {
            var items = 
                _context.Archives
                    .AsQueryable();

            #region ParentBy: 특정 부모 키 값(int, string)에 해당하는 리스트인지 확인
            // ParentBy 
            if (parentIdentifier is int parentId && parentId != 0)
            {
                items = items.Where(m => m.ParentId == parentId);
            }
            else if (parentIdentifier is string parentKey && !string.IsNullOrEmpty(parentKey))
            {
                items = items.Where(m => m.ParentKey == parentKey);
            }
            #endregion

            #region Search Mode: SearchField와 SearchQuery에 해당하는 데이터 검색
            // Search Mode
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (searchField == "Name")
                {
                    // Name
                    items = items
                        .Where(m => m.Name.Contains(searchQuery));
                }
                else if (searchField == "Title")
                {
                    // Title
                    items = items
                        .Where(m => m.Title.Contains(searchQuery));
                }
                else
                {
                    // All: 기타 더 검색이 필요한 컬럼이 있다면 추가 가능
                    items = items
                        .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery));
                }
            } 
            #endregion

            // 총 레코드 수 계산
            var totalCount = await items.CountAsync();

            #region Sorting: 어떤 열에 대해 정렬(None, Asc, Desc)할 것인지 원하는 문자열로 지정
            // Sorting
            switch (sortOrder)
            {
                case "Name":
                    items = items
                        .OrderBy(m => m.Name).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                    break;
                case "NameDesc":
                    items = items
                        .OrderByDescending(m => m.Name).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                    break;
                case "Title":
                    items = items
                        .OrderBy(m => m.Title).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                    break;
                case "TitleDesc":
                    items = items
                        .OrderByDescending(m => m.Title).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                    break;
                default:
                    items = items
                        .OrderByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                    break;
            } 
            #endregion

            // Paging
            items = items.Skip(pageIndex * pageSize).Take(pageSize);

            return new ArticleSet<Archive, int>(await items.AsNoTracking().ToListAsync(), totalCount);
        }

        //[4][16] 답변: ReplyApp
        public async Task<Archive> AddAsync(Archive model, int parentRef, int parentStep, int parentRefOrder)
        {
            #region 답변 관련 기능 추가된 영역
            // 비집고 들어갈 자리: 부모글 순서보다 큰 글이 있다면(기존 답변 글이 있다면) 해당 글의 순서를 모두 1씩 증가 
            var replys = await _context.Archives
                .Where(m => m.Ref == parentRef && m.RefOrder > parentRefOrder)
                .ToListAsync();
            foreach (var item in replys)
            {
                item.RefOrder = item.RefOrder + 1;
                try
                {
                    _context.Archives.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
                }
            }

            model.Ref = parentRef; // 답변 글의 Ref(그룹)은 부모 글의 Ref를 그대로 저장 
            model.Step = parentStep + 1; // 어떤 글의 답변 글이기에 들여쓰기 1 증가 
            model.RefOrder = parentRefOrder + 1; // 부모글의 바로 다음번 순서로 보여지도록 설정 
            #endregion

            model.Created = DateTime.UtcNow;

            try
            {
                _context.Archives.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
            }

            return model;
        }

        //[4][17] 답변: ArchiveApp
        public async Task<Archive> AddAsync(Archive model, int parentId)
        {
            #region 답변 관련 기능 추가된 영역
            //[0] 변수 선언
            var maxRefOrder = 0;
            var maxAnswerNum = 0;
            var parentRef = 0;
            var parentStep = 0;
            var parentRefOrder = 0;

            //[1] 부모글(답변의 대상)의 답변수(AnswerNum)를 1증가
            var parent = await GetByIdAsync(parentId);
            if (parent != null)
            {
                parentRef = parent?.Ref ?? 0;
                parentStep = parent?.Step ?? 0;
                parentRefOrder = parent?.RefOrder ?? 0;

                parent.AnswerNum = parent.AnswerNum + 1;

                await EditAsync(parent);
            }

            //[2] 동일 레벨의 답변이라면, 답변 순서대로 RefOrder를 설정. 같은 글에 대해서 답변을 두 번 이상하면 먼저 답변한 게 위에 나타나게 한다.
            var tempMaxRefOrder = await _context.Archives.Where(m => m.ParentNum == parentId).DefaultIfEmpty().MaxAsync(m => m == null ? 0 : m.RefOrder);
            var sameGroup = await _context.Archives.Where(m => m.ParentNum == parentId && m.RefOrder == tempMaxRefOrder).FirstOrDefaultAsync();
            if (sameGroup != null)
            {
                maxRefOrder = sameGroup.RefOrder;
                maxAnswerNum = sameGroup.AnswerNum;
            }
            else
            {
                var tmpParent = await _context.Archives.Where(m => m.Id == parentId).SingleOrDefaultAsync();
                if (tmpParent != null)
                {
                    maxRefOrder = tmpParent.RefOrder; 
                }
            }

            //[3] 중간에 답변달 때(비집고 들어갈 자리 마련): 부모글 순서보다 큰 글이 있다면(기존 답변 글이 있다면) 해당 글의 순서를 모두 1씩 증가 
            var replys = await _context.Archives.Where(m => m.Ref == parentRef && m.RefOrder > (maxRefOrder + maxAnswerNum)).ToListAsync();
            foreach (var item in replys)
            {
                item.RefOrder = item.RefOrder + 1;
                try
                {
                    _context.Entry(item).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
                }
            }

            //[4] 최종 저장
            model.Ref = parentRef; // 답변 글의 Ref(그룹)은 부모 글의 Ref를 그대로 저장 
            model.Step = parentStep + 1; // 어떤 글의 답변 글이기에 들여쓰기 1 증가 
            model.RefOrder = (maxRefOrder + maxAnswerNum + 1); // 부모글의 바로 다음번 순서로 보여지도록 설정 

            model.ParentNum = parentId;
            model.AnswerNum = 0;
            #endregion

            model.Created = DateTime.UtcNow;

            try
            {
                _context.Archives.Add(model);
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
        public async Task<ArticleSet<Archive, long>> GetByAsync<TParentIdentifier>(
            FilterOptions<TParentIdentifier> options)
        {
            var items = _context.Archives.AsQueryable();

            #region ParentBy: 특정 부모 키 값(int, string)에 해당하는 리스트인지 확인
            if (options.ChildMode)
            {
                // ParentBy 
                if (options.ParentIdentifier is int parentId && parentId != 0)
                {
                    //items = items.Where(m => m.ParentId == parentId);
                }
                else if (options.ParentIdentifier is string parentKey && !string.IsNullOrEmpty(parentKey))
                {
                    //items = items.Where(m => m.ParentKey == parentKey);
                }
            }
            #endregion

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
                    else if (options.SearchField == "Title")
                    {
                        // Title
                        items = items.Where(m => m.Title.Contains(searchQuery));
                    }
                    else if (options.SearchField == "Content")
                    {
                        // Title
                        items = items.Where(m => m.Content.Contains(searchQuery));
                    }
                    else
                    {
                        // All: 기타 더 검색이 필요한 컬럼이 있다면 추가 가능
                        items = items.Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Content.Contains(searchQuery));
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
                            items = items.OrderBy(m => m.Name).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                            break;
                        case "NameDesc":
                            //items = items.OrderByDescending(m => m.Name);
                            items = items.OrderByDescending(m => m.Name).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                            break;
                        case "TitleAsc":
                            //items = items.OrderBy(m => m.Title);
                            items = items.OrderBy(m => m.Title).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                            break;
                        case "TitleDesc":
                            //items = items.OrderByDescending(m => m.Title);
                            items = items.OrderByDescending(m => m.Title).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                            break;
                        default:
                            //items = items.OrderByDescending(m => m.Id);
                            items = items.OrderByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                            break;
                    }
                }
            }
            #endregion

            // Paging
            items = items.Skip(options.PageIndex * options.PageSize).Take(options.PageSize);

            return new ArticleSet<Archive, long>(await items.AsNoTracking().ToListAsync(), totalCount);
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
