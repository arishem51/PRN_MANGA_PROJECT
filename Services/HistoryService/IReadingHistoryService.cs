using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Services.HistoryService
{
    public interface IReadingHistoryService
    {
        Task AddOrUpdateHistoryAsync(string userId, int mangaId, int chapterId);
        Task<List<ReadingHistory>> GetAllHistoryAsync(string userId, int pageNumber, int pageSize);
        Task<List<ReadingHistory>> GetAllHistoryAsync(string userId);

        Task<int> TotalHistory(string userId);  
    }
}
