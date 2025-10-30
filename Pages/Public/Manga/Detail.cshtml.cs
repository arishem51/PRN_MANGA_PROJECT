using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using System.Security.Claims;

namespace PRN_MANGA_PROJECT.Pages.Public.Manga
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context)
        {
            _context = context;
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

        public IActionResult OnGet(int mangaId)
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
            if (userId == null) {
                return RedirectToPage("/Auth/Login");
            }
            readingHistory = _context.ReadingHistories.FirstOrDefault(r => r.MangaId == mangaId && r.UserId == userId);

            if(readingHistory == null)
            {
                readingContinueId = null;
            }
            else
            {
                readingContinueId = readingHistory.ChapterId;
            }

            firstChapterId = Manga.Chapters.OrderBy(c => c.ChapterNumber).FirstOrDefault().Id;
            lastestChapterId = Manga.Chapters.OrderByDescending(c => c.ChapterNumber).FirstOrDefault().Id;
            return Page();
        }

    }
}
