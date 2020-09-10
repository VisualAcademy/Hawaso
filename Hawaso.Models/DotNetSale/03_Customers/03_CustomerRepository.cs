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
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DotNetSaleCoreDbContext _context;
        private readonly ILogger _logger;

        public CustomerRepository(DotNetSaleCoreDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(CustomerRepository));
        }

        // 입력
        public async Task<Customer> AddAsync(Customer model)
        {
            try
            {
                _context.Customers.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"에러 발생({nameof(AddAsync)}): " + e.Message);
            }

            return model;
        }

        // 출력
        public async Task<List<Customer>> GetAllAsync()
        {
            var models = await _context.Customers.OrderByDescending(c => c.CustomerId)
                //.Include(c => c.State)
                .ToListAsync();
            return models; 
        }

        // 상세
        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _context.Customers
                //.Include(c => c.State)
                .SingleOrDefaultAsync(c => c.CustomerId == id);
        }

        // 수정
        public async Task<bool> EditAsync(Customer model)
        {
            try
            {
                _context.Customers.Attach(model);
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
                var model = await _context.Customers
                    //.Include(c => c.Orders)
                    .SingleOrDefaultAsync(c => c.CustomerId == id);
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
        public async Task<PagingResult<Customer>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Customers.CountAsync();
            var models = await _context.Customers
                .OrderByDescending(c => c.CustomerId)
                //.Include(c => c.State)
                //.Include(c => c.Orders)
                .Skip(pageIndex * pageSize) 
                .Take(pageSize)
                .ToListAsync();
            return new PagingResult<Customer>(models, totalRecords);
        }
    }
}
