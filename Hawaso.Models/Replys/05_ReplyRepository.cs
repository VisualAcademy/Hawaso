using Dul.Articles;
using Dul.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReplyApp.Models
{
    /// <summary>
    /// [6] Repository Class: ADO.NET or Dapper(Micro ORM) or Entity Framework Core(ORM)
    /// ~Repository, ~Provider, ~Data
    /// </summary>
    public class ReplyRepository : IReplyRepository, IDisposable
    {
        private readonly ReplyAppDbContext _context;
        private readonly ILogger _logger;

        public ReplyRepository(ReplyAppDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(ReplyRepository));
        }

        #region [6][1] 입력: AddAsync
        //[6][1] 입력: AddAsync
        public async Task<Reply> AddAsync(Reply model)
        {
            #region Reply 기능 추가
            // 현재테이블의 Ref의 Max값 가져오기
            int maxRef = 1;
            //int? max = _context.Replys.Max(m => m.Ref);
            int? max = await _context.Replys.DefaultIfEmpty().MaxAsync(m => m == null ? 0 : m.Ref);
            if (max.HasValue)
            {
                maxRef = (int)max + 1;
            }

            model.Ref = maxRef; // 참조 글(부모 글, 그룹 번호)
            model.Step = 0; // 들여쓰기(처음 글을 0으로 초기화)
            model.RefOrder = 0; // 참조(그룹) 순서
            #endregion

            try
            {
                _context.Replys.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
            }

            return model;
        }
        #endregion

        #region [6][2] 출력: GetAllAsync
        //[6][2] 출력: GetAllAsync
        public async Task<List<Reply>> GetAllAsync()
        {
            // 학습 목적으로... InMemory 데이터베이스에선 사용 금지 
            //return await _context.Replys.FromSqlRaw<Reply>("Select * From dbo.Replys Order By Id Desc") 
            return await _context.Replys.OrderByDescending(m => m.Id)
                //.Include(m => m.ReplysComments)
                .ToListAsync();
        }
        #endregion

        #region [6][3] 상세: GetByIdAsync
        //[6][3] 상세
        public async Task<Reply> GetByIdAsync(int id)
        {
            var model = await _context.Replys
                //.Include(m => m.ReplysComments)
                .SingleOrDefaultAsync(m => m.Id == id);

            // ReadCount++
            if (model != null)
            {
                model.ReadCount = model.ReadCount + 1;
                _context.Replys.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                _context.SaveChanges();
            }

            return model;
        }
        #endregion

        #region [6][4] 수정: UpdateAsync
        //[6][4] 수정: UpdateAsync
        public async Task<bool> EditAsync(Reply model)
        {
            try
            {
                _context.Replys.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(EditAsync)}): {e.Message}");
            }

            return false;
        }
        public async Task<bool> UpdateAsync(Reply model)
        {
            try
            {
                //_context.Replys.Attach(model);
                //_context.Entry(model).State = EntityState.Modified;
                _context.Update(model);
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(UpdateAsync)}): {e.Message}");
            }

            return false;
        }
        #endregion

        #region [6][5] 삭제: DeleteAsync
        //[6][5] 삭제
        public async Task<bool> DeleteAsync(int id)
        {
            //var model = await _context.Replys.SingleOrDefaultAsync(m => m.Id == id);
            try
            {
                var model = await _context.Replys.FindAsync(id);
                //_context.Replys.Remove(model);
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

        //[6][6] 페이징
        public async Task<PagingResult<Reply>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Replys.CountAsync();
            var models = await _context.Replys
                .OrderByDescending(m => m.Id)
                //.Include(m => m.ReplysComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Reply>(models, totalRecords);
        }

        //[6][7] 부모
        public async Task<PagingResult<Reply>> GetAllByParentIdAsync(
            int pageIndex,
            int pageSize,
            int parentId)
        {
            var totalRecords = await _context.Replys.Where(m => m.ParentId == parentId).CountAsync();
            var models = await _context.Replys
                .Where(m => m.ParentId == parentId)
                .OrderByDescending(m => m.Id)
                //.Include(m => m.ReplysComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Reply>(models, totalRecords);
        }

        //[6][8] 상태
        public async Task<Tuple<int, int>> GetStatus(int parentId)
        {
            var totalRecords = await _context.Replys.Where(m => m.ParentId == parentId).CountAsync();
            var pinnedRecords = await _context.Replys.Where(m => m.ParentId == parentId && m.IsPinned == true).CountAsync();

            return new Tuple<int, int>(pinnedRecords, totalRecords); // (2, 10)
        }

        //[6][9] 부모 삭제
        public async Task<bool> DeleteAllByParentId(int parentId)
        {
            try
            {
                var models = await _context.Replys.Where(m => m.ParentId == parentId).ToListAsync();

                foreach (var model in models)
                {
                    _context.Replys.Remove(model);
                }

                return (await _context.SaveChangesAsync() > 0 ? true : false);

            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(DeleteAllByParentId)}): {e.Message}");
            }

            return false;
        }

        //[6][10] 검색
        public async Task<PagingResult<Reply>> SearchAllAsync(
            int pageIndex,
            int pageSize,
            string searchQuery)
        {
            var totalRecords = await _context.Replys
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Content.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Replys
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Content.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                //.Include(m => m.ReplysComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Reply>(models, totalRecords);
        }

        //[6][11] 부모 검색
        public async Task<PagingResult<Reply>> SearchAllByParentIdAsync(
            int pageIndex,
            int pageSize,
            string searchQuery,
            int parentId)
        {
            var totalRecords = await _context.Replys.Where(m => m.ParentId == parentId)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") || m.Title.Contains(searchQuery) || m.Content.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Replys.Where(m => m.ParentId == parentId)
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Content.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                //.Include(m => m.ReplysComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Reply>(models, totalRecords);
        }

        //[6][12] 통계
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
                var cnt = _context.Replys.AsEnumerable().Where(
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

        //[6][13] 부모 페이징
        public async Task<PagingResult<Reply>> GetAllByParentKeyAsync(
            int pageIndex,
            int pageSize,
            string parentKey)
        {
            var totalRecords = await _context.Replys.Where(m => m.ParentKey == parentKey).CountAsync();
            var models = await _context.Replys
                .Where(m => m.ParentKey == parentKey)
                .OrderByDescending(m => m.Id)
                //.Include(m => m.ReplysComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Reply>(models, totalRecords);
        }

        //[6][14] 부모 검색
        public async Task<PagingResult<Reply>> SearchAllByParentKeyAsync(
            int pageIndex,
            int pageSize,
            string searchQuery,
            string parentKey)
        {
            var totalRecords = await _context.Replys.Where(m => m.ParentKey == parentKey)
                .Where(m => EF.Functions.Like(m.Name, $"%{searchQuery}%") || m.Title.Contains(searchQuery) || m.Content.Contains(searchQuery))
                .CountAsync();
            var models = await _context.Replys.Where(m => m.ParentKey == parentKey)
                .Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery) || m.Content.Contains(searchQuery))
                .OrderByDescending(m => m.Id)
                //.Include(m => m.ReplysComments)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Reply>(models, totalRecords);
        }

        //[6][15] 리스트(페이징, 검색, 정렬)
        public async Task<ArticleSet<Reply, int>> GetArticlesAsync<TParentIdentifier>(
            int pageIndex,
            int pageSize,
            string searchField,
            string searchQuery,
            string sortOrder,
            TParentIdentifier parentIdentifier)
        {
            //var items = from m in _context.Replys select m; // 쿼리 구문(Query Syntax)
            //var items = _context.Replys.Select(m => m); // 메서드 구문(Method Syntax)
            var items = _context.Replys.AsQueryable();

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
                    items = items.Where(m => m.Name.Contains(searchQuery));
                }
                else if (searchField == "Title")
                {
                    // Title
                    items = items.Where(m => m.Title.Contains(searchQuery));
                }
                else
                {
                    // All: 기타 더 검색이 필요한 컬럼이 있다면 추가 가능
                    items = items.Where(m => m.Name.Contains(searchQuery) || m.Title.Contains(searchQuery));
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
                    //items = items.OrderBy(m => m.Name);
                    items = items.OrderBy(m => m.Name).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                    break;
                case "NameDesc":
                    //items = items.OrderByDescending(m => m.Name);
                    items = items.OrderByDescending(m => m.Name).ThenByDescending(m => m.Ref).ThenBy(m => m.RefOrder);
                    break;
                case "Title":
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
            #endregion

            // Paging
            items = items.Skip(pageIndex * pageSize).Take(pageSize);

            return new ArticleSet<Reply, int>(await items.ToListAsync(), totalCount);
        }

        //[6][16] 답변
        public async Task<Reply> AddAsync(Reply model, int parentRef, int parentStep, int parentOrder)
        {
            #region 답변 관련 기능 추가된 영역
            // 비집고 들어갈 자리: 부모글 순서보다 큰 글이 있다면(기존 답변 글이 있다면) 해당 글의 순서를 모두 1씩 증가 
            var replys = await _context.Replys.Where(m => m.Ref == parentRef && m.RefOrder > parentOrder).ToListAsync();
            foreach (var item in replys)
            {
                item.RefOrder = item.RefOrder + 1;
                try
                {
                    _context.Replys.Attach(item);
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
            model.RefOrder = parentOrder + 1; // 부모글의 바로 다음번 순서로 보여지도록 설정 
            #endregion

            try
            {
                _context.Replys.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
            }

            return model;
        }

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
