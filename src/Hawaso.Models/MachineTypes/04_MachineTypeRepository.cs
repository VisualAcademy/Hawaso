using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MachineTypeApp.Models
{
    public class MachineTypeRepository : IMachineTypeRepository
    {
        private readonly MachineTypeDbContext _context;

        public MachineTypeRepository(MachineTypeDbContext context)
        {
            this._context = context;
        }

        // 입력
        public async Task<MachineType> AddMachineTypeAsync(MachineType model)
        {
            _context.MachineTypes.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        // 출력
        public async Task<List<MachineType>> GetMachineTypesAsync()
        {
            return await _context.MachineTypes.OrderBy(m => m.Id).ToListAsync();
        }

        // 상세
        public async Task<MachineType> GetMachineTypeAsync(int id)
        {
            return await _context.MachineTypes.Where(m => m.Id == id).SingleOrDefaultAsync();
        }

        // 수정
        public async Task<MachineType> EditMachineTypeAsync(MachineType model)
        {
            try
            {
                _context.MachineTypes.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {

            }
            return model;
        }

        // 삭제 
        public async Task DeleteMachineTypeAsync(int id)
        {
            var machineType = await _context.MachineTypes.Where(m => m.Id == id).SingleOrDefaultAsync();
            if (machineType != null)
            {
                _context.MachineTypes.Remove(machineType);
                await _context.SaveChangesAsync();
            }
        }

        // 페이징 
        public async Task<ArticleSet<MachineType, int>> GetAllAsync(int pageIndex, int pageSize)
        {
            var totalCount = await _context.MachineTypes.CountAsync(); // 총 레코드 수
            var items = await _context.MachineTypes
                .OrderBy(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ArticleSet<MachineType, int>(items, totalCount); // 페이징된 데이터 + 카운트
        }
    }
}
