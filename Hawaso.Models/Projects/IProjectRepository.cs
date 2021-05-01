using Dul.Articles;
using Dul.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Models
{
    /// <summary>
    /// Repository Interface
    /// </summary>
    public interface IProjectRepository
    {
        Task<Project> AddProjectAsync(Project model);           // 입력
        Task<List<Project>> GetProjectsAsync();                 // 출력
        Task<Project> GetProjectByIdAsync(int id);              // 상세
        Task<Project> EditProjectAsync(Project model);          // 수정
        Task DeleteProjectAsync(int id);                        // 삭제

        Task<PagingResult<Project>> GetAllAsync(
            int pageIndex, int pageSize);                       // 페이징

        // 필터링: 비동기 방식
        Task<ArticleSet<Project, int>> GetArticles<TParentIdentifier>(
            int pageIndex,
            int pageSize,
            string searchField,
            string searchQuery,
            string sortOrder,
            TParentIdentifier parentIdentifier);

        // 필터링: 동기 방식
        ArticleSet<Project, int> GetArticlesSync<TParentIdentifier>(
            int pageIndex,
            int pageSize,
            string searchField,
            string searchQuery,
            string sortOrder,
            TParentIdentifier parentIdentifier);
    }
}
