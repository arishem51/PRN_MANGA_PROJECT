using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;

namespace PRN_MANGA_PROJECT.Areas.Admin.Pages.Chapter
{
    public class ChapterModel : PageModel
    {
        private readonly IChapterService _chapterService;
        private readonly IMangaService _mangaService;

        public ChapterModel(IChapterService chapterService, IMangaService mangaService)
        {
            _chapterService = chapterService;
            _mangaService = mangaService;
        }

        public IList<ChapterViewModel> Chapters { get; private set; } = new List<ChapterViewModel>();
        public MangaViewModel? SelectedManga { get; private set; }
        public int? MangaId { get; private set; }

        public async Task OnGetAsync(int? mangaId = null)
        {
            MangaId = mangaId;
            
            if (mangaId.HasValue)
            {
                SelectedManga = await _mangaService.GetMangaByIdAsync(mangaId.Value);
                var chapters = await _chapterService.GetChaptersByMangaIdAsync(mangaId.Value);
                Chapters = chapters.ToList();
            }
            else
            {
                var chapters = await _chapterService.GetAllChaptersAsync();
                Chapters = chapters.ToList();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteChapterRequest request)
        {
            try
            {
                await _chapterService.DeleteChapterAsync(request.Id);
                return new JsonResult(new { success = true, message = "Chapter deleted successfully" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }

    public class DeleteChapterRequest
    {
        public int Id { get; set; }
    }
}

