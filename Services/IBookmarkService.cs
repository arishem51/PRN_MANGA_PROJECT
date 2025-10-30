using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories;

namespace PRN_MANGA_PROJECT.Services
{
    public interface IBookmarkService
    {
        Task<bool> SaveBookmarkAsync(string userId, int mangaId); // Lưu bookmark
        Task<IEnumerable<Bookmark>> GetBookmarksByUserIdAsync(string userId); // Lấy bookmark của người dùng
        Task<bool> IsMangaBookmarkedAsync(string userId, int mangaId); // Kiểm tra xem manga đã bookmark chưa
        Task<bool> RemoveBookmarkAsync(string userId, int mangaId); // Bỏ lưu bookmark
    }

   
    
   
    

}
