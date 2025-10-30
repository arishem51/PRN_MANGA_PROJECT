using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Services;
using PRN_MANGA_PROJECT.Models.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages.Public
{
    public class BookMarkModel : PageModel
    {
        private readonly IBookmarkService _bookmarkService;

        public BookMarkModel(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        // Danh sách manga đã lưu
        public IEnumerable<PRN_MANGA_PROJECT.Models.Entities.Manga> BookmarkedMangas { get; set; } = new List<PRN_MANGA_PROJECT.Models.Entities.Manga>();

        // Khi người dùng vào trang
        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy UserId từ đăng nhập

            if (!string.IsNullOrEmpty(userId))
            {
                // Lấy danh sách manga đã lưu của người dùng
                var bookmarks = await _bookmarkService.GetBookmarksByUserIdAsync(userId);
                // Chuyển đổi từ bookmark thành manga
                BookmarkedMangas = bookmarks.Select(b => b.Manga);
            }
            else
            {
                // Nếu người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }
    }
}
