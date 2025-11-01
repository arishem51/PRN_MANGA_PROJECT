using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;
using System.Security.Claims;


namespace PRN_MANGA_PROJECT.Pages.Public.Manga
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookmarkService _bookmarkService;

        public bool IsBookmarked { get; set; } = false;
        public DetailModel(ApplicationDbContext context, IBookmarkService bookmarkService)
        {
            _context = context;
            _bookmarkService = bookmarkService;
        }


        [BindProperty]
        public PRN_MANGA_PROJECT.Models.Entities.Manga Manga { get; set; } = default!;

        [BindProperty]
        public List<Models.Entities.Chapter> chapters { get; set; } = new List<Models.Entities.Chapter>();

        [BindProperty]
        public List<Models.Entities.Tag> Tags { get; set; } = new List<Models.Entities.Tag>();


        [BindProperty]
        public ReadingHistory readingHistory { get; set; }
        [BindProperty]
        public int? readingContinueId { get; set; }
        public int? firstChapterId { get; set; }
        public int? lastestChapterId { get; set; }

        public async Task<IActionResult> OnGet(int mangaId)
        {
            if(mangaId == null)
            {
                return RedirectToPage("/Public/Error");

            }
            Manga = _context.Mangas
                            .Include(m => m.Chapters)
                            .Include(m => m.MangaTags)
                                .ThenInclude(mt => mt.Tag)
                            .FirstOrDefault(m => m.Id == mangaId);

            if (Manga == null)
            {
                return RedirectToPage("/Public/Error");
            }

            chapters = Manga.Chapters.OrderByDescending(c => int.TryParse(c.ChapterNumber, out int num) ? num : 0).ToList();
            Tags = Manga.MangaTags.Select(mt => mt.Tag).ToList();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                IsBookmarked = await _bookmarkService.IsMangaBookmarkedAsync(userId, mangaId);
            }


            if (userId != null) {
                readingHistory = _context.ReadingHistories.FirstOrDefault(r => r.MangaId == mangaId && r.UserId == userId);

                if (readingHistory == null)
                {
                    readingContinueId = null;
                }
                else
                {
                    readingContinueId = readingHistory.ChapterId;
                }
            }
            

            if (Manga.Chapters.Any())
            {
                firstChapterId = Manga.Chapters.OrderBy(c => c.ChapterNumber).FirstOrDefault().Id;
                lastestChapterId = Manga.Chapters.OrderByDescending(c => c.ChapterNumber).FirstOrDefault().Id;
            }
            else
            {
                firstChapterId = null;
                lastestChapterId = null;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveBookmarkAsync(int mangaId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var result = await _bookmarkService.SaveBookmarkAsync(userId, mangaId);
            if (result)
            {
                TempData["Message"] = "Truyện đã được lưu vào danh sách yêu thích.";
            }
            else
            {
                TempData["Message"] = "Truyện này đã được lưu rồi.";
            }
            IsBookmarked = true; 
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveBookmarkAsync(int mangaId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var result = await _bookmarkService.RemoveBookmarkAsync(userId, mangaId);
            if (result)
            {
                TempData["Message"] = "Truyện đã được xóa khỏi danh sách yêu thích.";
            }
            else
            {
                TempData["Message"] = "Không thể xóa truyện này khỏi danh sách yêu thích.";
            }
            IsBookmarked = false;
            return RedirectToPage();
        }
    }
}
