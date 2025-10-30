using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories
{
    public class BookmarkRepository : BaseRepository<Bookmark>, IBookmarkRepository
    {
        public BookmarkRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Bookmark>> GetBookmarksByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(b => b.Manga)
                .ThenInclude(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Bookmark?> GetBookmarkByUserAndMangaAsync(string userId, int mangaId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(b => b.UserId == userId && b.MangaId == mangaId);
        }
        public async Task AddBookmarkAsync(Bookmark bookmark)
        {
            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveBookmark(Bookmark bookmark)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
        }
    }
}
