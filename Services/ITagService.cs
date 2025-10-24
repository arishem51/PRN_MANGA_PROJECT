using PRN_MANGA_PROJECT.Models.ViewModels;

namespace PRN_MANGA_PROJECT.Services
{
    public interface ITagService
    {
        /// <summary>
        /// Lấy danh sách tag đang hoạt động (hiện lên popup chọn tag)
        /// </summary>
        Task<IEnumerable<TagViewModel>> GetActiveTagsAsync();

        /// <summary>
        /// Lấy thông tin tag kèm danh sách manga (nếu có)
        /// </summary>
        Task<TagViewModel?> GetTagWithMangaCountAsync(int id);
    }
}
