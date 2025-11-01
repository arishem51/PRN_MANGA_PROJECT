using Microsoft.AspNetCore.SignalR;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories.ReadingHistoryRepository;

namespace PRN_MANGA_PROJECT.Services.HistoryService
{
    public class ReadingHistoryService : IReadingHistoryService
    {
        private readonly IHistoryRepository _repository;
        private readonly IHubContext<ReadingHistoryHub> _hub;


        public ReadingHistoryService(IHistoryRepository repository, IHubContext<ReadingHistoryHub> hub)
        {
            _repository = repository;
            _hub = hub;
        }

        public async Task AddOrUpdateHistoryAsync(string userId, int mangaId, int chapterId)
        {
            var history = await _repository.GetHistoryAsync(userId, mangaId);

            if (history != null)
            {
                history.ChapterId = chapterId;
                history.ReadAt = DateTime.Now;
                await _repository.UpdateAsync(history);
            }
            else
            {
                var newHistory = new ReadingHistory
                {
                    UserId = userId,
                    MangaId = mangaId,
                    ChapterId = chapterId,
                    ReadAt = DateTime.Now
                };
                await _repository.AddAsync(newHistory);
            }
            await _repository.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("LoadReadingHistory");
        }

        public async Task<List<ReadingHistory>> GetAllHistoryAsync(string userId , int pageNumber, int pageSize)
        {
            return await _repository.GetHistoryAsync(userId , pageNumber , pageSize);
        }

        public async Task<List<ReadingHistory>> GetAllHistoryAsync(string userId)
        {
            return await _repository.GetAllHistoryAsync(userId);
        }

        public async Task<int> TotalHistory(string userId)
        {
            return await _repository.CountHistoryAsync(userId);
        }
    }
}
