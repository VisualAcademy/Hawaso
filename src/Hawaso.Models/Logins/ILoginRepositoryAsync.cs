using Dul.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Models
{
    /// <summary>
    /// Repository Interface
    /// </summary>
    public interface ILoginRepositoryAsync
    {
        Task<Login> AddAsync(Login model); // 입력
        Task<List<Login>> GetAllAsync(); // 출력
        Task<Login> GetByIdAsync(int id); // 상세
        Task<bool> EditAsync(Login model); // 수정
        Task<bool> DeleteAsync(int id); // 삭제
        Task<PagingResult<Login>> GetAllAsync(int pageIndex, int pageSize); // 페이징
    }
}
