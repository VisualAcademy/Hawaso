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
    /// [6] Repository Class: ADO.NET 또는 Dapper 또는 Entity Framework Core 
    /// </summary>
    public class ProductRepositoryAsync : IProductRepositoryAsync
    {
        private readonly DotNetSaleCoreDbContext _context;
        private readonly ILogger _logger;

        public ProductRepositoryAsync(DotNetSaleCoreDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(ProductRepositoryAsync));
        }

        // 입력
        public async Task<Product> AddAsync(Product model)
        {
            try
            {
                _context.Products.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR({nameof(AddAsync)}): " + e.Message);
            }

            return model;
        }

        // 출력
        public async Task<List<Product>> GetAllAsync()
        {
            var models = await _context.Products.OrderByDescending(c => c.ProductId)
                //.Include(c => c.Category)
                .ToListAsync();
            return models;
        }

        // 상세
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                //.Include(c => c.Category)
                .SingleOrDefaultAsync(c => c.ProductId == id);
        }

        //[6][4] 수정
        public async Task<bool> EditAsync(Product model)
        {
            try
            {
                //_context.Products.Attach(model);
                //_context.Entry(model).State = EntityState.Modified;
                _context.Update(model);
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR({nameof(EditAsync)}): " + e.Message);
            }

            return false;
        }

        //[6][5] 삭제
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var model = await _context.Products.SingleOrDefaultAsync(c => c.ProductId == id);
                _context.Remove(model);
                return (await _context.SaveChangesAsync() > 0 ? true : false);
            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR({nameof(DeleteAsync)}): " + e.Message);
            }

            return false;
        }

        // 페이징
        public async Task<PagingResult<Product>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Products.CountAsync();
            var models = await _context.Products
                .OrderByDescending(c => c.ProductId)
                //.Include(c => c.Category)
                //.Include(c => c.Reviews)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Product>(models, totalRecords);
        }

        // 부모
        public async Task<PagingResult<Product>> GetAllByParentIdAsync(int pageIndex, int pageSize, int parentId)
        {
            var totalRecords = await _context.Products.Where(m => m.CategoryId == parentId).CountAsync();
            var models = await _context.Products
                .Where(m => m.CategoryId == parentId)
                .OrderByDescending(c => c.ProductId)
                //.Include(c => c.Category)
                //.Include(c => c.Reviews)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Product>(models, totalRecords);
        }
    }
}
