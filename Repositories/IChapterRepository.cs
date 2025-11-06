using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;

namespace PRN_MANGA_PROJECT.Repositories
{
    public interface IChapterRepository : IBaseRepository<Chapter>
    {
        Task<IEnumerable<Chapter>> GetChaptersByMangaIdAsync(int mangaId);
        Task<PagedResult<Chapter>> GetChaptersByMangaIdPagedAsync(int mangaId, PaginationParams paginationParams);
        Task<Chapter?> GetChapterWithImagesAsync(int id);
        Task<Chapter?> GetChapterWithImagesForAdminAsync(int id);
        Task<Chapter?> GetNextChapterAsync(int mangaId, int currentChapterId);
        Task<Chapter?> GetPreviousChapterAsync(int mangaId, int currentChapterId);
    }
}
