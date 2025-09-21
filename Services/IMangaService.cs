using PRN_MANGA_PROJECT.Models.ViewModels;

namespace PRN_MANGA_PROJECT.Services
{
    public interface IMangaService
    {
        Task<IEnumerable<MangaViewModel>> GetAllMangaAsync();
        Task<MangaViewModel?> GetMangaByIdAsync(int id);
        Task<IEnumerable<MangaViewModel>> SearchMangaAsync(string searchTerm);
        Task<IEnumerable<MangaViewModel>> GetMangaByTagAsync(int tagId);
        Task<IEnumerable<MangaViewModel>> GetMangaByStatusAsync(string status);
        Task<IEnumerable<MangaViewModel>> GetPopularMangaAsync(int count);
        Task<IEnumerable<MangaViewModel>> GetRecentMangaAsync(int count);
        Task<MangaViewModel> CreateMangaAsync(MangaViewModel manga);
        Task<MangaViewModel> UpdateMangaAsync(MangaViewModel manga);
        Task DeleteMangaAsync(int id);
        Task<bool> BookmarkMangaAsync(string userId, int mangaId);
        Task<bool> RemoveBookmarkAsync(string userId, int mangaId);
        Task<bool> IsBookmarkedAsync(string userId, int mangaId);
    }
}
