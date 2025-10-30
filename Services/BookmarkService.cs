using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories;

namespace PRN_MANGA_PROJECT.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IBookmarkRepository _bookmarkRepository;

        public BookmarkService(IBookmarkRepository bookmarkRepository)
        {
            _bookmarkRepository = bookmarkRepository;
        }

        // Thêm bookmark vào cơ sở dữ liệu
        public async Task<bool> SaveBookmarkAsync(string userId, int mangaId)
        {
            // Kiểm tra xem manga đã được lưu chưa
            var existingBookmark = await _bookmarkRepository.GetBookmarkByUserAndMangaAsync(userId, mangaId);
            if (existingBookmark != null)
            {
                return false; // Nếu manga đã được lưu rồi thì không thêm nữa
            }

            // Thêm bookmark mới vào cơ sở dữ liệu
            var bookmark = new Bookmark
            {
                UserId = userId,
                MangaId = mangaId,
                CreatedAt = DateTime.UtcNow
            };

            await _bookmarkRepository.AddBookmarkAsync(bookmark);
            return true; // Trả về true nếu thêm thành công
        }

        // Lấy tất cả các bookmark của người dùng
        public async Task<IEnumerable<Bookmark>> GetBookmarksByUserIdAsync(string userId)
        {
            return await _bookmarkRepository.GetBookmarksByUserIdAsync(userId);
        }

        // Kiểm tra xem manga đã được người dùng lưu chưa
        public async Task<bool> IsMangaBookmarkedAsync(string userId, int mangaId)
        {
            var bookmark = await _bookmarkRepository.GetBookmarkByUserAndMangaAsync(userId, mangaId);
            return bookmark != null; // Trả về true nếu manga đã được lưu
        }
        public async Task<bool> RemoveBookmarkAsync(string userId, int mangaId)
        {
            var bookmark = await _bookmarkRepository.GetBookmarkByUserAndMangaAsync(userId, mangaId);
            if (bookmark == null)
            {
                return false; // Nếu không tìm thấy bookmark, không thể bỏ lưu
            }

            await _bookmarkRepository.RemoveBookmark(bookmark); // Đảm bảo gọi async
            return true;
        }

    }
}
