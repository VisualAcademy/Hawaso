using Dul.Articles;
using Dul.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Models
{
    /// <summary>
    /// Repository Class: ADO.NET 또는 Dapper 또는 Entity Framework Core(EF Core)
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        private readonly HawasoDbContext _context;

        public ProjectRepository(HawasoDbContext context)
        {
            _context = context;
        }

        // 입력
        public async Task<Project> AddProjectAsync(Project model)
        {
            _context.Projects.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        // 출력 
        public async Task<List<Project>> GetProjectsAsync()
        {
            return await _context.Projects.OrderByDescending(m => m.Id).ToListAsync();
        }

        // 상세
        public async Task<Project> GetProjectByIdAsync(int id)
        {
            return await _context.Projects.Where(m => m.Id == id).SingleOrDefaultAsync();
        }

        // 수정
        public async Task<Project> EditProjectAsync(Project model)
        {
            try
            {
                _context.Entry(model).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {

            }
            return model;
        }

        // 삭제
        public async Task DeleteProjectAsync(int id)
        {
            var model = await _context.Projects.Where(m => m.Id == id).SingleOrDefaultAsync();
            if (model != null)
            {
                _context.Projects.Remove(model);
                await _context.SaveChangesAsync();
            }
        }

        // 페이징
        public async Task<PagingResult<Project>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Projects.CountAsync();
            var articles = await _context.Projects
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Project>(articles, totalRecords);
        }

        // 필터링: 비동기 방식
        public async Task<ArticleSet<Project, int>> GetArticles<TParentIdentifier>(
            int pageIndex,
            int pageSize,
            string searchField,
            string searchQuery,
            string sortOrder,
            TParentIdentifier parentIdentifier)
        {
            var items = _context.Projects.Select(m => m); // 메서드 구문(Method Syntax)

            // Search Mode
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (searchField == "Title")
                {
                    // Name
                    items = items.Where(m => m.Title.Contains(searchQuery));
                }
                else if (searchField == "Content")
                {
                    // Title
                    items = items.Where(m => m.Content.Contains(searchQuery));
                }
                else
                {
                    // All
                    items = items.Where(
                        m => m.Title.Contains(searchQuery) ||
                        m.Content.Contains(searchQuery) ||
                        m.ManufacturerName.Contains(searchQuery) ||
                        m.Status.Contains(searchQuery) ||
                        m.Created.ToString().Contains(searchQuery) ||
                        m.MachineQuantity.ToString().Contains(searchQuery) ||
                        m.MediaQuantity.ToString().Contains(searchQuery));
                }
            }

            var totalCount = await items.CountAsync();

            // Sorting
            switch (sortOrder)
            {
                case "Title":
                    items = items.OrderBy(m => m.Title);
                    break;
                case "TitleDesc":
                    items = items.OrderByDescending(m => m.Title);
                    break;
                case "Create":
                    items = items.OrderBy(m => m.Created);
                    break;
                case "CreateDesc":
                    items = items.OrderByDescending(m => m.Created);
                    break;
                case "Manufacturer":
                    items = items.OrderBy(m => m.ManufacturerName);
                    break;
                case "ManufacturerDesc":
                    items = items.OrderByDescending(m => m.ManufacturerName);
                    break;
                case "Machine":
                    items = items.OrderBy(m => m.MachineQuantity);
                    break;
                case "MachineDesc":
                    items = items.OrderByDescending(m => m.MachineQuantity);
                    break;
                case "Media":
                    items = items.OrderBy(m => m.MediaQuantity);
                    break;
                case "MediaDesc":
                    items = items.OrderByDescending(m => m.MediaQuantity);
                    break;
                case "Status":
                    items = items.OrderBy(m => m.Status);
                    break;
                case "StatusDesc":
                    items = items.OrderByDescending(m => m.Status);
                    break;
                default:
                    items = items.OrderByDescending(m => m.Id);
                    break;
            }

            // Paging
            items = items.Skip(pageIndex * pageSize).Take(pageSize);

            return new ArticleSet<Project, int>(await items.ToListAsync(), totalCount);
        }

        // 필터링: 동기 방식
        public ArticleSet<Project, int> GetArticlesSync<TParentIdentifier>(int pageIndex,
            int pageSize,
            string searchField,
            string searchQuery,
            string sortOrder,
            TParentIdentifier parentIdentifier)
        {
            var items = _context.Projects.Select(m => m); // 메서드 구문(Method Syntax)

            // Search Mode
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (searchField == "Title")
                {
                    // Name
                    items = items.Where(m => m.Title.Contains(searchQuery));
                }
                else if (searchField == "Content")
                {
                    // Title
                    items = items.Where(m => m.Content.Contains(searchQuery));
                }
                else
                {
                    // All
                    items = items.Where(
                        m => m.Title.Contains(searchQuery) ||
                        m.Content.Contains(searchQuery) ||
                        m.ManufacturerName.Contains(searchQuery) ||
                        m.Status.Contains(searchQuery) ||
                        m.Created.ToString().Contains(searchQuery) ||
                        m.MachineQuantity.ToString().Contains(searchQuery) ||
                        m.MediaQuantity.ToString().Contains(searchQuery));
                }
            }

            var totalCount = items.Count();

            // Sorting
            switch (sortOrder)
            {
                case "Title":
                    items = items.OrderBy(m => m.Title);
                    break;
                case "TitleDesc":
                    items = items.OrderByDescending(m => m.Title);
                    break;
                case "Create":
                    items = items.OrderBy(m => m.Created);
                    break;
                case "CreateDesc":
                    items = items.OrderByDescending(m => m.Created);
                    break;
                case "Manufacturer":
                    items = items.OrderBy(m => m.ManufacturerName);
                    break;
                case "ManufacturerDesc":
                    items = items.OrderByDescending(m => m.ManufacturerName);
                    break;
                case "Machine":
                    items = items.OrderBy(m => m.MachineQuantity);
                    break;
                case "MachineDesc":
                    items = items.OrderByDescending(m => m.MachineQuantity);
                    break;
                case "Media":
                    items = items.OrderBy(m => m.MediaQuantity);
                    break;
                case "MediaDesc":
                    items = items.OrderByDescending(m => m.MediaQuantity);
                    break;
                case "Status":
                    items = items.OrderBy(m => m.Status);
                    break;
                case "StatusDesc":
                    items = items.OrderByDescending(m => m.Status);
                    break;
                default:
                    items = items.OrderByDescending(m => m.Id);
                    break;
            }

            // Paging
            items = items.Skip(pageIndex * pageSize).Take(pageSize);

            return new ArticleSet<Project, int>(items.ToList(), totalCount);
        }
    }
}
