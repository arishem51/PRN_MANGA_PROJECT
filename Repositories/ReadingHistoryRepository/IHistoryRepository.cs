using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories.ReadingHistoryRepository
{
    public interface IHistoryRepository
    {
        Task<ReadingHistory?> GetHistoryAsync(string userId , int mangaId);
        Task<List<ReadingHistory>?> GetHistoryAsync(string userId , int pageNumber, int pageSize);
        Task<List<ReadingHistory>> GetAllHistoryAsync(string userId);
        Task<int> CountHistoryAsync(string userId);
        Task AddAsync(ReadingHistory history);
        Task UpdateAsync(ReadingHistory history);
        Task SaveChangesAsync();
    }
}
