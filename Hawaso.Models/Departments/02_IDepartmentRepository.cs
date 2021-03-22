using Dul.Articles;
using Dul.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Models
{
    public interface IDepartmentCrudRepository<T> : ICrudRepositoryBase<DepartmentModel, long>
    {
        Task<bool> EditAsync(T model); // 수정
        Task<T> AddAsync(
            T model,
            int parentRef,
            int parentStep,
            int parentRefOrder); // 답변(기본: ReplyApp)
        Task<T> AddAsync(
            T model,
            int parentId); // 답변(고급: DepartmentApp)

        // 페이징
        Task<PagingResult<T>> GetAllAsync(
            int pageIndex,
            int pageSize);

        // 부모 Id
        Task<PagingResult<T>> GetAllByParentIdAsync(
            int pageIndex,
            int pageSize,
            int parentId);

        // 부모 Key
        Task<PagingResult<T>> GetAllByParentKeyAsync(
            int pageIndex,
            int pageSize,
            string parentKey);

        // 검색
        Task<PagingResult<T>> SearchAllAsync(
            int pageIndex,
            int pageSize,
            string searchQuery);

        // 검색 + 부모 Id
        Task<PagingResult<T>> SearchAllByParentIdAsync(
            int pageIndex,
            int pageSize,
            string searchQuery,
            int parentId);

        // 검색 + 부모 Key
        Task<PagingResult<T>> SearchAllByParentKeyAsync(
            int pageIndex,
            int pageSize,
            string searchQuery,
            string parentKey);
    }

    public interface IDepartmentRepository : IDepartmentCrudRepository<DepartmentModel>
    {
        Task<ArticleSet<DepartmentModel, long>> GetByAsync<TParentIdentifier>(FilterOptions<TParentIdentifier> options);
        Task<Tuple<int, int>> GetStatus(int parentId);
        Task<bool> DeleteAllByParentId(int parentId);
        Task<SortedList<int, double>> GetMonthlyCreateCountAsync();
        // 강의 이외에 추가적인 API가 필요하다면 이곳에 기록(예를 들어, 시작일부터 종료일까지의 데이터 검색)
    }
}
