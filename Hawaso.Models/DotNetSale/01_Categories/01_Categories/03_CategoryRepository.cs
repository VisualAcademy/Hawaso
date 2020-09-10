using Dul.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetSaleCore.Models
{
    /// <summary>
    /// Repository Class: ADO.NET 또는 Dapper 또는 Entity Framework Core 
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DotNetSaleCoreDbContext _context;
        private readonly ILogger _logger;

        public CategoryRepository(DotNetSaleCoreDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(CategoryRepository));
        }

        // 입력
        public async Task<Category> AddAsync(Category model)
        {
            try
            {
               _context.Categories.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"에러 발생({nameof(AddAsync)}): " + e.Message);
            }

            return model;
        }

        // 출력
        public async Task<List<Category>> GetAllAsync()
        {
            var models = await _context.Categories.OrderByDescending(c => c.CategoryId)
                .ToListAsync();
            return models;
        }

        // 상세
        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                .SingleOrDefaultAsync(c => c.CategoryId == id);
        }

        // 수정
        public async Task<bool> EditAsync(Category model)
        {
            try
            {
                _context.Categories.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger.LogError($"에러 발생({nameof(EditAsync)}): " + e.Message);
            }
            return false;
        }

        // 삭제
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var model = await _context.Categories
                    .SingleOrDefaultAsync(c => c.CategoryId == id);
                _context.Remove(model);
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger.LogError($"에러 발생({nameof(DeleteAsync)}): " + e.Message);
            }
            return false;
        }

        // 페이징
        public async Task<PagingResult<Category>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Categories.CountAsync();
            var models = await _context.Categories
                .OrderByDescending(c => c.CategoryId)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PagingResult<Category>(models, totalRecords);
        }
    }
}
