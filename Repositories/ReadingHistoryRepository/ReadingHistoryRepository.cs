using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories.ReadingHistoryRepository
{
    public class ReadingHistoryRepository : IHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ReadingHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReadingHistory?> GetHistoryAsync(string userId, int mangaId)
        {
            return await _context.ReadingHistories.FirstOrDefaultAsync(c => c.UserId == userId && c.MangaId == mangaId);
        }

        public async Task AddAsync(ReadingHistory history)
        {
            await _context.ReadingHistories.AddAsync(history);
        }

        public async Task UpdateAsync(ReadingHistory history)
        {
            _context.ReadingHistories.Update(history);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<ReadingHistory>?> GetHistoryAsync(string userId , int pageNumber, int pageSize)
        {
            return await _context.ReadingHistories.Include(r => r.Manga).Include(r => r.Chapter)
                   .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.ReadAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }

        public async Task<int> CountHistoryAsync(string userId)
        {
            return await _context.ReadingHistories
                 .CountAsync(r => r.UserId == userId);
        }

        public async Task<List<ReadingHistory>> GetAllHistoryAsync(string userId)
        {
            return await _context.ReadingHistories.Include(r => r.Manga).Include(r => r.Chapter)
                   .Where(c => c.UserId == userId).OrderByDescending(r => r.ReadAt).ToListAsync();
        }

    }
}
