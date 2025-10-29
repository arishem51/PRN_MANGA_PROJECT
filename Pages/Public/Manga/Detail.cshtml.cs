using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;

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

        public IActionResult OnGet(int mangaId)
        {
            Manga = _context.Mangas
                            .Include(m => m.Chapters)
                            .Include(m => m.MangaTags)
                                .ThenInclude(mt => mt.Tag)
                            .FirstOrDefault(m => m.Id == mangaId);

            if (Manga == null)
            {
                return NotFound();
            }

            chapters = Manga.Chapters.OrderByDescending(c => int.TryParse(c.ChapterNumber, out int num) ? num : 0).ToList();
            Tags = Manga.MangaTags.Select(mt => mt.Tag).ToList();

            return Page();
        }

    }
}
