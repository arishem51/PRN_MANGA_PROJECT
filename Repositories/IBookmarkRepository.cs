using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories
{
    public interface IBookmarkRepository : IBaseRepository<Bookmark>
    {
        Task<IEnumerable<Bookmark>> GetBookmarksByUserIdAsync(string userId);
        Task<Bookmark?> GetBookmarkByUserAndMangaAsync(string userId, int mangaId);
    }
}
