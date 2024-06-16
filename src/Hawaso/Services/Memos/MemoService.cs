namespace Hawaso.Services.Memos
{
    public class MemoService : IMemoService
    {
        private readonly IMemoRepository _memoRepository;

        public MemoService(IMemoRepository memoRepository)
        {
            _memoRepository = memoRepository;
        }

        //public Task<IEnumerable<Memo>> GetAllMemosAsync() => _memoRepository.GetAllAsync();
        public Task<IEnumerable<Memo>> GetAllMemosAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Memo> GetMemoByIdAsync(long id) => _memoRepository.GetByIdAsync(id);

        public Task CreateMemoAsync(Memo memo) => _memoRepository.AddAsync(memo);

        public Task UpdateMemoAsync(Memo memo) => _memoRepository.UpdateAsync(memo);

        public Task DeleteMemoAsync(long id) => _memoRepository.DeleteAsync(id);

    }
}
