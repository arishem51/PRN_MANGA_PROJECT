using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tag>> GetActiveTagsAsync()
        {
            return await _dbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag?> GetTagWithMangaCountAsync(int id)
        {
            return await _dbSet
                .Include(t => t.MangaTags)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
