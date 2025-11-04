using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;

namespace PRN_MANGA_PROJECT.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public int TotalStories { get; set; }
        public int TotalChapters { get; set; }
        public int TotalViews { get; set; }
        public List<MangaView> Top5Manga { get; set; } = new();
        public class MangaView
        {
            public string Title { get; set; } = "";
            public int ViewCount { get; set; }
        }
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Tổng số liệu
            TotalStories = await _context.Mangas.CountAsync();
            TotalChapters = await _context.Chapters.CountAsync();
            TotalViews = await _context.ReadingHistories.CountAsync();
            Top5Manga = await _context.Mangas
              .Select(m => new MangaView
              {
                  Title = m.Title,
                  ViewCount = _context.ReadingHistories
                      .Count(r => r.MangaId == m.Id)
              })
              .OrderByDescending(x => x.ViewCount)
              .Take(5)
              .ToListAsync();
        }
        
    }
}
