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
        public PRN_MANGA_PROJECT.Models.Entities.Manga Manga { get; set; }

        [BindProperty]
        public List<Chapter> chapters { get; set; } = new List<Chapter>();

        public IActionResult OnGet(int mangaId)
        {
            Manga = _context.Mangas.Include(m => m.Chapters).FirstOrDefault();
            chapters = Manga.Chapters.ToList();

            if (Manga == null)
            {
                return NotFound();
            }

            return Page();
        }

    }
}
