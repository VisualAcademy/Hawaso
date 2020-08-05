using Dul.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetSaleCore.Models
{
    /// <summary>
    /// Repository Interface
    /// </summary>
    public interface ICustomerRepository
    {
        Task<Customer> AddAsync(Customer model); // 입력
        Task<List<Customer>> GetAllAsync(); // 출력
        Task<Customer> GetByIdAsync(int id); // 상세
        Task<bool> EditAsync(Customer model); // 수정
        Task<bool> DeleteAsync(int id); // 삭제
        Task<PagingResult<Customer>> GetAllAsync(int pageIndex, int pageSize); // 페이징
    }
}
