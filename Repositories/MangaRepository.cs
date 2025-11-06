using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories
{
    public class MangaRepository : BaseRepository<Manga>, IMangaRepository
    {
        public MangaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Manga>> GetMangaWithTagsAsync()
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Where(m => m.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Manga>> GetAllMangaWithTagsAsync()
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Include(m => m.Chapters)
                .ToListAsync();
        }

        public async Task<Manga?> GetMangaWithTagsByIdAsync(int id)
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Include(m => m.Chapters.Where(c => c.IsActive))
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
        }

        public async Task<IEnumerable<Manga>> SearchMangaAsync(string searchTerm)
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Where(m => m.IsActive && 
                    (m.Title.Contains(searchTerm) || 
                     m.Author!.Contains(searchTerm) || 
                     m.Artist!.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<IEnumerable<Manga>> GetMangaByTagAsync(int tagId)
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Where(m => m.IsActive && m.MangaTags.Any(mt => mt.TagId == tagId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Manga>> GetMangaByStatusAsync(string status)
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Where(m => m.IsActive && m.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Manga>> GetPopularMangaAsync(int count)
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.ReadingHistories.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Manga>, int)> GetMangaPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Mangas
                .OrderByDescending(m => m.CreatedAt).Take(50);

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<(IEnumerable<Manga>, int)> SearchMangaPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            var query = _context.Mangas
                .Where(m => m.Title.Contains(searchTerm) || m.Author.Contains(searchTerm) || m.Artist.Contains(searchTerm))
                .OrderByDescending(m => m.CreatedAt).Take(3);

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<IEnumerable<Manga>> GetRecentMangaAsync(int count)
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Manga>, int)> GetMangaByTagPagedAsync(int tagId, int pageNumber, int pageSize)
        {
            var query = _context.Mangas
                .Include(m => m.MangaTags)
                .ThenInclude(mt => mt.Tag)
                .Where(m => m.IsActive && m.MangaTags.Any(mt => mt.TagId == tagId))
                .OrderByDescending(m => m.CreatedAt)
                .Take(3); 

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

    }
}
