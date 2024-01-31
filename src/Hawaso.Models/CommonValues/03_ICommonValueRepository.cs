using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Models.CommonValues
{
    /// <summary>
    /// [3] Repository Interface
    /// </summary>
    public interface ICommonValueRepository : ICommonValueCrudRepository<CommonValue>
    {
        Task<Tuple<int, int>> GetStatus(int parentId);
        Task<bool> DeleteAllByParentId(int parentId);
        Task<SortedList<int, double>> GetMonthlyCreateCountAsync();

        Task<string> FindValueByTest(string text);
    }
}
