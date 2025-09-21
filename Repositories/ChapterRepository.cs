using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories
{
    public class ChapterRepository : BaseRepository<Chapter>, IChapterRepository
    {
        public ChapterRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Chapter>> GetChaptersByMangaIdAsync(int mangaId)
        {
            return await _dbSet
                .Include(c => c.ChapterImages)
                .Where(c => c.MangaId == mangaId && c.IsActive)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();
        }

        public async Task<Chapter?> GetChapterWithImagesAsync(int id)
        {
            return await _dbSet
                .Include(c => c.ChapterImages.OrderBy(ci => ci.PageNumber))
                .Include(c => c.Manga)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<Chapter?> GetNextChapterAsync(int mangaId, int currentChapterId)
        {
            return await _dbSet
                .Where(c => c.MangaId == mangaId && c.Id > currentChapterId && c.IsActive)
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Chapter?> GetPreviousChapterAsync(int mangaId, int currentChapterId)
        {
            return await _dbSet
                .Where(c => c.MangaId == mangaId && c.Id < currentChapterId && c.IsActive)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
        }
    }
}
