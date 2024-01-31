using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineTypeApp.Models
{
    public interface IMachineTypeRepository
    {
        Task<MachineType> AddMachineTypeAsync(MachineType model);                       // 입력
        Task<List<MachineType>> GetMachineTypesAsync();                                 // 출력
        Task<MachineType> GetMachineTypeAsync(int id);                                  // 상세
        Task<MachineType> EditMachineTypeAsync(MachineType model);                      // 수정
        Task DeleteMachineTypeAsync(int id);                                            // 삭제
        Task<ArticleSet<MachineType, int>> GetAllAsync(int pageIndex, int pageSize);    // 페이징
    }
}
