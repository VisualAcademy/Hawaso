namespace VisualAcademy.Models.Replys
{
    /// <summary>
    /// [4] Repository Interface, Provider Interface
    /// </summary>
    public interface IReplyRepository : IReplyCrudRepository<Reply>
    {
        Task<Tuple<int, int>> GetStatus(int parentId);
        Task<bool> DeleteAllByParentId(int parentId);
        Task<SortedList<int, double>> GetMonthlyCreateCountAsync();
    }
}
