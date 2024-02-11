using Dul.Articles;
using Dul.Domain.Common;

namespace VisualAcademy.Models.BannedTypes;

/// <summary>
/// BannedTypeRepository는 IBannedTypeRepository와 IDisposable 인터페이스를 구현하는 클래스입니다.
/// </summary>
/// <remarks>
/// BannedTypeRepository 생성자는 DB Context와 Logger를 주입받습니다.
/// </remarks>
public class BannedTypeRepository(BannedTypeAppDbContext context, ILoggerFactory loggerFactory) : IBannedTypeRepository, IDisposable
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(nameof(BannedTypeRepository));

    #region [4][1] 입력: AddAsync
    //[4][1] 입력: AddAsync
    /// <summary>
    /// AddAsync 메서드는 신규 BannedTypeModel 인스턴스를 데이터베이스에 추가합니다.
    /// </summary>
    public async Task<BannedTypeModel> AddAsync(BannedTypeModel model)
    {
        model.CreatedAt = DateTime.UtcNow;

        try
        {
            context.BannedTypes.Add(model);
            await context.SaveChangesAsync();
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
    /// <summary>
    /// GetAllAsync 메서드는 모든 BannedTypeModel 인스턴스를 데이터베이스에서 가져옵니다.
    /// </summary>
    public async Task<List<BannedTypeModel>> GetAllAsync() => await context.BannedTypes.OrderByDescending(m => m.Id).ToListAsync();
    #endregion

    #region [4][3] 상세: GetByIdAsync
    //[4][3] 상세: GetByIdAsync
    /// <summary>
    /// GetByIdAsync 메서드는 주어진 Id에 해당하는 BannedTypeModel 인스턴스를 데이터베이스에서 가져옵니다.
    /// </summary>
    public async Task<BannedTypeModel> GetByIdAsync(long id)
    {
        var model = await context.BannedTypes.SingleOrDefaultAsync(m => m.Id == id);

        return model!;
    }
    #endregion

    #region [4][4] 수정: UpdateAsync
    //[4][4] 수정: UpdateAsync
    /// <summary>
    /// EditAsync 메서드는 주어진 BannedTypeModel 인스턴스의 변경사항을 데이터베이스에 반영합니다.
    /// </summary>
    public async Task<bool> EditAsync(BannedTypeModel model)
    {
        try
        {
            //_context.BannedTypes.Attach(model);
            context.Entry(model).State = EntityState.Modified;
            return (await context.SaveChangesAsync() > 0 ? true : false);
        }
        catch (Exception e)
        {
            _logger?.LogError($"ERROR({nameof(EditAsync)}): {e.Message}");
        }

        return false;
    }

    /// <summary>
    /// UpdateAsync 메서드는 주어진 BannedTypeModel 인스턴스의 변경사항을 데이터베이스에 반영합니다.
    /// </summary>
    public async Task<bool> UpdateAsync(BannedTypeModel model)
    {
        try
        {
            var m = context.BannedTypes.Find(model.Id);

            m!.Name = model.Name;
            m.Active = model.Active;

            context.Update(m);
            return (await context.SaveChangesAsync() > 0 ? true : false);
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
    /// <summary>
    /// 지정된 ID를 가진 BannedType를 삭제합니다.
    /// </summary>
    /// <param name="id">삭제할 BannedType의 ID</param>
    /// <returns>작업 성공 여부</returns>
    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            var model = await context.BannedTypes.FindAsync(id);
            context.Remove(model!);
            return await context.SaveChangesAsync() > 0;
        }
        catch (Exception ಠ_ಠ) // Disapproval Look
        {
            _logger?.LogError($"ERROR({nameof(DeleteAsync)}): {ಠ_ಠ.Message}");
        }

        return false;
    }
    #endregion

    #region [4][6] 페이징: GetAllAsync
    //[4][6] 페이징: GetAllAsync()
    /// <summary>
    /// 모든 BannedType를 페이지 단위로 반환합니다.
    /// </summary>
    /// <param name="pageIndex">반환할 페이지 인덱스</param>
    /// <param name="pageSize">페이지 당 반환할 개수</param>
    /// <returns>페이지 결과</returns>
    public async Task<PagingResult<BannedTypeModel>> GetAllAsync(int pageIndex, int pageSize)
    {
        var totalRecords = await context.BannedTypes.CountAsync();
        var models = await context.BannedTypes
            .OrderByDescending(m => m.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagingResult<BannedTypeModel>(models, totalRecords);
    }
    #endregion

    #region [4][7] 부모: GetAllByParentIdAsync
    //[4][7] 부모
    /// <summary>
    /// 지정된 부모 ID를 가진 BannedType를 페이지 단위로 반환합니다.
    /// </summary>
    /// <param name="pageIndex">반환할 페이지 인덱스</param>
    /// <param name="pageSize">페이지 당 반환할 개수</param>
    /// <param name="parentId">부모 ID</param>
    /// <returns>페이지 결과</returns>
    public async Task<PagingResult<BannedTypeModel>> GetAllByParentIdAsync(
        int pageIndex,
        int pageSize,
        int parentId)
    {
        var totalRecords = await context.BannedTypes
            //.Where(m => m.ParentId == parentId)
            .CountAsync();
        var models = await context.BannedTypes
            //.Where(m => m.ParentId == parentId)
            .OrderByDescending(m => m.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagingResult<BannedTypeModel>(models, totalRecords);
    }
    #endregion

    #region [4][8] 상태: GetStatus
    //[4][8] 상태
    /// <summary>
    /// 지정된 부모 ID를 가진 BannedType의 상태를 반환합니다.
    /// </summary>
    /// <param name="parentId">부모 ID</param>
    /// <returns>고정된 기록과 전체 기록의 수</returns>
    public async Task<Tuple<int, int>> GetStatus(int parentId)
    {
        var totalRecords = await context.BannedTypes
            //.Where(m => m.ParentId == parentId)
            .CountAsync();
        var pinnedRecords = await context.BannedTypes
            //.Where(m => m.ParentId == parentId && m.IsPinned == true)
            .CountAsync();

        return new Tuple<int, int>(pinnedRecords, totalRecords); // (2, 10)
    }
    #endregion

    #region [4][9] 부모 삭제: DeleteAllByParentId
    //[4][9] 부모 삭제
    /// <summary>
    /// 지정된 부모 ID를 가진 모든 BannedType를 삭제합니다.
    /// </summary>
    /// <param name="parentId">부모 ID</param>
    /// <returns>작업 성공 여부</returns>
    public async Task<bool> DeleteAllByParentId(int parentId)
    {
        try
        {
            var models = await context.BannedTypes
                //.Where(m => m.ParentId == parentId)
                .ToListAsync();

            foreach (var model in models)
            {
                context.BannedTypes.Remove(model);
            }

            return (await context.SaveChangesAsync() > 0 ? true : false);

        }
        catch (Exception e)
        {
            _logger?.LogError($"ERROR({nameof(DeleteAllByParentId)}): {e.Message}");
        }

        return false;
    }
    #endregion

    #region [4][10] 검색: SearchAllAsync
    //[4][10] 검색
    /// <summary>
    /// 주어진 쿼리로 BannedType를 검색하고, 그 결과를 페이지 단위로 반환합니다.
    /// </summary>
    /// <param name="pageIndex">반환할 페이지 인덱스</param>
    /// <param name="pageSize">페이지 당 반환할 개수</param>
    /// <param name="searchQuery">검색 쿼리</param>
    /// <returns>페이지 결과</returns>
    public async Task<PagingResult<BannedTypeModel>> SearchAllAsync(
        int pageIndex,
        int pageSize,
        string searchQuery)
    {
        var totalRecords = await context.BannedTypes
            .Where(m => m.Name!.Contains(searchQuery)
            //|| m.Title.Contains(searchQuery) 
            //|| m.Title.Contains(searchQuery)
            )
            .CountAsync();
        var models = await context.BannedTypes
            .Where(m => m.Name!.Contains(searchQuery)
            //|| m.Title.Contains(searchQuery) 
            //|| m.Title.Contains(searchQuery)
            )
            .OrderByDescending(m => m.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagingResult<BannedTypeModel>(models, totalRecords);
    }
    #endregion

    #region [4][11] 부모 검색: SearchAllByParentIdAsync
    //[4][11] 부모 검색
    /// <summary>
    /// 주어진 쿼리로 지정된 부모 ID를 가진 BannedType를 검색하고, 그 결과를 페이지 단위로 반환합니다.
    /// </summary>
    /// <param name="pageIndex">반환할 페이지 인덱스</param>
    /// <param name="pageSize">페이지 당 반환할 개수</param>
    /// <param name="searchQuery">검색 쿼리</param>
    /// <param name="parentId">부모 ID</param>
    /// <returns>페이지 결과</returns>
    public async Task<PagingResult<BannedTypeModel>> SearchAllByParentIdAsync(
        int pageIndex,
        int pageSize,
        string searchQuery,
        int parentId)
    {
        var totalRecords = await context.BannedTypes
            //.Where(m => m.ParentId == parentId)
            .Where(m => EF.Functions.Like(m.Name!, $"%{searchQuery}%")
            //|| m.Title.Contains(searchQuery) 
            )
            .CountAsync();
        var models = await context.BannedTypes
            //.Where(m => m.ParentId == parentId)
            .Where(m => m.Name!.Contains(searchQuery)
            //|| m.Title.Contains(searchQuery) 
            )
            .OrderByDescending(m => m.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagingResult<BannedTypeModel>(models, totalRecords);
    }
    #endregion

    #region [4][12] 통계: GetMonthlyCreateCountAsync
    //[4][12] 통계
    /// <summary>
    /// 지난 12개월 동안 생성된 기록의 월별 통계를 반환합니다.
    /// </summary>
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
            var cnt = context.BannedTypes.AsEnumerable()
                .Where(
                    m => m?.CreatedAt != null
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
    #endregion

    #region [4][13] 부모 페이징: GetAllByParentKeyAsync
    //[4][13] 부모 페이징
    /// <summary>
    /// 부모 키를 기준으로 페이지화된 BannedType 목록을 반환합니다.
    /// </summary>
    public async Task<PagingResult<BannedTypeModel>> GetAllByParentKeyAsync(
        int pageIndex,
        int pageSize,
        string parentKey)
    {
        var totalRecords = await context.BannedTypes
            //.Where(m => m.ParentKey == parentKey)
            .CountAsync();
        var models = await context.BannedTypes
            //.Where(m => m.ParentKey == parentKey)
            .OrderByDescending(m => m.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagingResult<BannedTypeModel>(models, totalRecords);
    }
    #endregion

    #region [4][14] 부모 검색: SearchAllByParentKeyAsync
    //[4][14] 부모 검색
    /// <summary>
    /// 부모 키를 기준으로 검색 쿼리가 적용된 페이지화된 BannedType 목록을 반환합니다.
    /// </summary>
    public async Task<PagingResult<BannedTypeModel>> SearchAllByParentKeyAsync(
        int pageIndex,
        int pageSize,
        string searchQuery,
        string parentKey)
    {
        var totalRecords = await context.BannedTypes
            //.Where(m => m.ParentKey == parentKey)
            .Where(m => EF.Functions.Like(m.Name!, $"%{searchQuery}%")
            //|| m.Title.Contains(searchQuery) 
            )
            .CountAsync();
        var models = await context.BannedTypes
            //.Where(m => m.ParentKey == parentKey)
            .Where(m => m.Name!.Contains(searchQuery)
            //|| m.Title.Contains(searchQuery)
            )
            .OrderByDescending(m => m.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagingResult<BannedTypeModel>(models, totalRecords);
    }
    #endregion

    #region [4][15][1] 리스트(페이징, 검색, 정렬): GetAllAsync<TParentIdentifier>()
    //[4][15] 리스트(페이징, 검색, 정렬)
    /// <summary>
    /// 페이징, 검색, 정렬이 적용된 BannedType 목록을 반환합니다.
    /// </summary>
    public async Task<ArticleSet<BannedTypeModel, int>> GetAllAsync<TParentIdentifier>(int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier)
    {
        var items = context.BannedTypes.AsQueryable();

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
                    .Where(m => m.Name!.Contains(searchQuery)).AsNoTracking();
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
                    .Where(m => m.Name!.Contains(searchQuery)
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

        return new ArticleSet<BannedTypeModel, int>(await items.AsNoTracking().ToListAsync(), totalCount);
    }
    #endregion

    #region  [4][15][2] 리스트(페이징, 검색, 정렬): GetArticlesAsync<TParentIdentifier>()
    public async Task<ArticleSet<BannedTypeModel, int>> GetArticlesAsync<TParentIdentifier>(
        int pageIndex,
        int pageSize,
        string searchField,
        string searchQuery,
        string sortOrder,
        TParentIdentifier parentIdentifier)
    {
        return await GetAllAsync(pageIndex, pageSize, searchField, searchQuery, sortOrder, parentIdentifier);
    }
    #endregion

    #region [4][16] 답변: AddAsync(ReplyAsync)
    //[4][16] 답변: AddAsync(ReplyAsync)
    /// <summary>
    /// 답변을 추가합니다. 추가된 답변은 저장되며, 그 결과가 반환됩니다.
    /// </summary>
    public async Task<BannedTypeModel> AddAsync(BannedTypeModel model, int parentRef, int parentStep, int parentRefOrder)
    {
        model.CreatedAt = DateTime.UtcNow;

        try
        {
            context.BannedTypes.Add(model);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
        }

        return model;
    }
    #endregion

    #region [4][17] 답변: AddAsync(ReplyAsync)
    //[4][17]  답변: AddAsync(ReplyAsync)
    /// <summary>
    /// BannedType에 답변을 추가합니다. 추가된 답변은 저장되며, 그 결과가 반환됩니다.
    /// </summary>
    public async Task<BannedTypeModel> AddAsync(BannedTypeModel model, int parentId)
    {
        model.CreatedAt = DateTime.UtcNow;

        try
        {
            context.BannedTypes.Add(model);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger?.LogError($"ERROR({nameof(AddAsync)}): {e.Message}");
        }

        return model;
    }
    #endregion

    #region [4][18] 검색: GetByAsync()
    //[4][18] 검색: GetByAsync()
    /// <summary>
    /// 특정 필터 옵션에 따라 데이터를 검색하고 그 결과를 반환합니다.
    /// </summary>
    public async Task<ArticleSet<BannedTypeModel, long>> GetByAsync<TParentIdentifier>(
        FilterOptions<TParentIdentifier> options)
    {
        var items = context.BannedTypes.AsQueryable();

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
                    items = items.Where(m => m.Name!.Contains(searchQuery));
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
                    items = items.Where(m => m.Name!.Contains(searchQuery)
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

        return new ArticleSet<BannedTypeModel, long>(await items.AsNoTracking().ToListAsync(), totalCount);
    }
    #endregion

    #region Dispose
    // https://docs.microsoft.com/ko-kr/dotnet/api/system.gc.suppressfinalize?view=net-5.0
    /// <summary>
    /// Dispose 메서드는 사용한 리소스를 청소하며, 이 개체를 가비지 컬렉터(GC)가 따로 자동으로 정리하지 않게 요청합니다.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose 메서드는 리소스를 정리합니다. 이 메서드는 Dispose 메서드에 의해 호출됩니다.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (context != null)
            {
                context.Dispose(); //_context = null;
            }
        }
    }
    #endregion
}