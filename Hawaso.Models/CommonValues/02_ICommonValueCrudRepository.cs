using Dul.Articles;
using Dul.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Models.CommonValues
{
    /// <summary>
    /// [2] Generic Repository Interface
    /// </summary>
    public interface ICommonValueCrudRepository<T>
    {
        Task<T> AddAsync(T model); // 입력
        Task<T> AddAsync(T model, int parentRef, int parentStep, int parentOrder); // 답변
        Task<List<T>> GetAllAsync(); // 출력
        Task<T> GetByIdAsync(int id); // 상세
        Task<bool> EditAsync(T model); // 수정
        Task<bool> DeleteAsync(int id); // 삭제
        Task<PagingResult<T>> GetAllAsync(int pageIndex, int pageSize); // 페이징
        Task<PagingResult<T>> GetAllByParentIdAsync(int pageIndex, int pageSize, int parentId); // 부모
        Task<PagingResult<T>> GetAllByParentKeyAsync(int pageIndex, int pageSize, string parentKey); // 부모
        Task<PagingResult<T>> SearchAllAsync(int pageIndex, int pageSize, string searchQuery); // 검색
        Task<PagingResult<T>> SearchAllByParentIdAsync(int pageIndex, int pageSize, string searchQuery, int parentId); // 검색+부모
        Task<PagingResult<T>> SearchAllByParentKeyAsync(int pageIndex, int pageSize, string searchQuery, string parentKey); // 검색+부모

        Task<ArticleSet<T, int>> GetArticles<TParentIdentifier>(int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier);
    }
}
