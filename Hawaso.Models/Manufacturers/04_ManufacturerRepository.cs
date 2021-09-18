using Dul.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Models
{
    public class ManufacturerRepository : IManufacturerRepository
    {
        private readonly ManufacturerDbContext _context;

        public ManufacturerRepository(ManufacturerDbContext context)
        {
            _context = context;
        }

        // 입력
        public async Task<Manufacturer> AddManufacturerAsync(Manufacturer manufacturer)
        {
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();
            return manufacturer;
        }

        // 출력
        public async Task<List<Manufacturer>> GetManufacturersAsync()
        {
            return await _context.Manufacturers.OrderBy(m => m.Id).ToListAsync();
        }

        // 상세
        public async Task<Manufacturer> GetManufacturerAsync(int id)
        {
            return await _context.Manufacturers.Where(m => m.Id == id).SingleOrDefaultAsync();
        }

        // 수정
        public async Task<Manufacturer> EditManufacturerAsync(Manufacturer manufacturer)
        {
            try
            {
                _context.Entry(manufacturer).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {

            }
            return manufacturer;
        }

        // 삭제 
        public async Task DeleteManufacturerAsync(int id)
        {
            var manufacturer = await _context.Manufacturers.Where(m => m.Id == id).SingleOrDefaultAsync();
            if (manufacturer != null)
            {
                _context.Manufacturers.Remove(manufacturer);
                await _context.SaveChangesAsync();
            }
        }

        // 페이징
        public async Task<List<Manufacturer>> GetAllAsync(
            int pageIndex, int pageSize = 10)
        {
            return await _context.Manufacturers
                .OrderBy(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // 페이징 
        public async Task<PagingResult<Manufacturer>> GetAllByPageAsync(int pageIndex, int pageSize)
        {
            var totalRecords = await _context.Manufacturers.CountAsync(); // 총 레코드 수
            var manufacturers = await _context.Manufacturers
                .OrderBy(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResult<Manufacturer>(manufacturers, totalRecords); // 페이징된 데이터 + 카운트
        }
    }
}
