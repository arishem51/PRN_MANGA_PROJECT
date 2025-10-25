using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;

namespace PRN_MANGA_PROJECT.Pages.Chapter
{
    public class ReaderModel : PageModel
    {
        private readonly ChapterUIService _chapterService;

        public ReaderModel(ChapterUIService chapterService)
        {
            _chapterService = chapterService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 0; // Changed from Page to PageNumber

        public ChapterViewModel? Chapter { get; set; }
        public int CurrentImageIndex { get; set; }
        public int TotalImages { get; set; }
        public int? PreviousChapterId { get; set; }
        public int? NextChapterId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Chapter = await _chapterService.GetChapterWithImagesAsync(Id);

            if (Chapter == null)
            {
                return NotFound();
            }

            if (Chapter.ChapterImages != null && Chapter.ChapterImages.Any())
            {
                TotalImages = Chapter.ChapterImages.Count;
                CurrentImageIndex = Math.Max(0, Math.Min(PageNumber, TotalImages - 1));
            }
            else
            {
                TotalImages = 1; // For text-based chapters, consider it one "page"
                CurrentImageIndex = 0;
            }

            PreviousChapterId = await _chapterService.GetPreviousChapterIdAsync(Id, Chapter.MangaId);
            NextChapterId = await _chapterService.GetNextChapterIdAsync(Id, Chapter.MangaId);

            return Page();
        }
    }
}
