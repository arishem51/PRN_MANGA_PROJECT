using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;

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
                .OrderBy(c => c.ChapterNumber ?? "")
                .ThenBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<PagedResult<Chapter>> GetChaptersByMangaIdPagedAsync(int mangaId, PaginationParams paginationParams)
        {
            var query = _dbSet
                .Include(c => c.ChapterImages)
                .Where(c => c.MangaId == mangaId && c.IsActive);

            // Apply sorting
            if (!string.IsNullOrEmpty(paginationParams.SortBy))
            {
                switch (paginationParams.SortBy.ToLower())
                {
                    case "title":
                        query = paginationParams.SortDescending 
                            ? query.OrderByDescending(c => c.Title)
                            : query.OrderBy(c => c.Title);
                        break;
                    case "createdat":
                        query = paginationParams.SortDescending 
                            ? query.OrderByDescending(c => c.CreatedAt)
                            : query.OrderBy(c => c.CreatedAt);
                        break;
                    case "pagenumber":
                        query = paginationParams.SortDescending 
                            ? query.OrderByDescending(c => c.PageCount)
                            : query.OrderBy(c => c.PageCount);
                        break;
                    default:
                        query = query.OrderBy(c => c.ChapterNumber ?? "")
                            .ThenBy(c => c.Id);
                        break;
                }
            }
            else
            {
                // Default sorting by chapter number
                query = query.OrderBy(c => c.ChapterNumber ?? "")
                    .ThenBy(c => c.Id);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var chapters = await query
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<Chapter>
            {
                Data = chapters,
                TotalCount = totalCount,
                Page = paginationParams.Page,
                PageSize = paginationParams.PageSize
            };
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
