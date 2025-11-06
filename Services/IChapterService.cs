using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;

namespace PRN_MANGA_PROJECT.Services
{
    public interface IChapterService
    {
        Task<IEnumerable<ChapterViewModel>> GetAllChaptersAsync();
        Task<ChapterViewModel?> GetChapterByIdAsync(int id);
        Task<IEnumerable<ChapterViewModel>> GetChaptersByMangaIdAsync(int mangaId);
        Task<PagedResult<ChapterViewModel>> GetChaptersByMangaIdPagedAsync(int mangaId, PaginationParams paginationParams);
        Task<ChapterViewModel?> GetChapterWithImagesAsync(int id);
        Task<ChapterViewModel?> GetChapterWithImagesForAdminAsync(int id);
        Task<ChapterViewModel?> GetNextChapterAsync(int mangaId, int currentChapterId);
        Task<ChapterViewModel?> GetPreviousChapterAsync(int mangaId, int currentChapterId);
        Task<ChapterViewModel> CreateChapterAsync(CreateChapterViewModel model);
        Task<ChapterViewModel> UpdateChapterAsync(int id, UpdateChapterViewModel model);
        Task<bool> DeleteChapterAsync(int id);
        Task<bool> ChapterExistsAsync(int id);
    }
}
