namespace Zero.Models
{
    /// <summary>
    /// [4] Repository Interface
    /// </summary>
    public interface IBriefingLogRepository : IBriefingLogCrudRepository<BriefingLog>
    {
        Task<Tuple<int, int>> GetStatus(int parentId);
        Task<bool> DeleteAllByParentId(int parentId);
        Task<SortedList<int, double>> GetMonthlyCreateCountAsync();
    }
}
