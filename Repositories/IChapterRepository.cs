using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories
{
    public interface IChapterRepository : IBaseRepository<Chapter>
    {
        Task<IEnumerable<Chapter>> GetChaptersByMangaIdAsync(int mangaId);
        Task<Chapter?> GetChapterWithImagesAsync(int id);
        Task<Chapter?> GetNextChapterAsync(int mangaId, int currentChapterId);
        Task<Chapter?> GetPreviousChapterAsync(int mangaId, int currentChapterId);
    }
}
