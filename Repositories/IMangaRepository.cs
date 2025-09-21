using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories
{
    public interface IMangaRepository : IBaseRepository<Manga>
    {
        Task<IEnumerable<Manga>> GetMangaWithTagsAsync();
        Task<Manga?> GetMangaWithTagsByIdAsync(int id);
        Task<IEnumerable<Manga>> SearchMangaAsync(string searchTerm);
        Task<IEnumerable<Manga>> GetMangaByTagAsync(int tagId);
        Task<IEnumerable<Manga>> GetMangaByStatusAsync(string status);
        Task<IEnumerable<Manga>> GetPopularMangaAsync(int count);
        Task<IEnumerable<Manga>> GetRecentMangaAsync(int count);
    }
}
