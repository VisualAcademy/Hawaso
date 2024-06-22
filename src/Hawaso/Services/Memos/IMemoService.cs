namespace Hawaso.Services.Memos;

public interface IMemoService
{
    Task<IEnumerable<Memo>> GetAllMemosAsync();
    Task<Memo> GetMemoByIdAsync(long id);
    Task CreateMemoAsync(Memo memo);
    Task UpdateMemoAsync(Memo memo);
    Task DeleteMemoAsync(long id);
}
