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
    /// Repository Class: ADO.NET 또는 Dapper 또는 Entity Framework Core 
    /// </summary>
    public class LoginRepositoryAsync : ILoginRepositoryAsync
    {
        private readonly LoginDbContext _context;
        private readonly ILogger _logger;

        public LoginRepositoryAsync(LoginDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(LoginRepositoryAsync));
        }

        // 입력
        public async Task<Login> AddAsync(Login model)
        {
            try
            {
               _context.Logins.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"에러 발생({nameof(AddAsync)}): " + e.Message);
            }

            return model;
        }

        // 출력
        public async Task<List<Login>> GetAllAsync()
        {
            var models = await _context.Logins.OrderByDescending(c => c.LoginId)
                .ToListAsync();
            return models;
        }

        // 상세
        public async Task<Login> GetByIdAsync(int id)
        {
            return await _context.Logins
                .SingleOrDefaultAsync(c => c.LoginId == id);
        }

        // 수정
        public async Task<bool> EditAsync(Login model)
        {
            try
            {
                _context.Logins.Attach(model);
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
                var model = await _context.Logins
                    .SingleOrDefaultAsync(c => c.LoginId == id);
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
        public async Task<PagingResult<Login>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Logins.CountAsync();
            var models = await _context.Logins
                .OrderByDescending(c => c.LoginId)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PagingResult<Login>(models, totalRecords);
        }
    }
}
