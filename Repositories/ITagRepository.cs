using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories
{
    public interface ITagRepository : IBaseRepository<Tag>
    {
        Task<IEnumerable<Tag>> GetActiveTagsAsync();
        Task<Tag?> GetTagWithMangaCountAsync(int id);
    }
}
