using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Areas.Admin.Pages.Chapter
{
    public class CreateModel : PageModel
    {
        private readonly IChapterService _chapterService;
        private readonly IMangaService _mangaService;

        public CreateModel(IChapterService chapterService, IMangaService mangaService)
        {
            _chapterService = chapterService;
            _mangaService = mangaService;
        }

        [BindProperty]
        public CreateChapterInputModel Input { get; set; } = new();

        public SelectList MangaList { get; set; } = new(new List<MangaViewModel>(), "Id", "Title");

        public async Task OnGetAsync(int? mangaId = null)
        {
            var mangas = await _mangaService.GetAllMangaAsync();
            MangaList = new SelectList(mangas, "Id", "Title", mangaId);
            
            if (mangaId.HasValue)
            {
                Input.MangaId = mangaId.Value;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var mangas = await _mangaService.GetAllMangaAsync();
                MangaList = new SelectList(mangas, "Id", "Title", Input.MangaId);
                return Page();
            }

            try
            {
                var chapterViewModel = new CreateChapterViewModel
                {
                    MangaId = Input.MangaId,
                    Title = Input.Title,
                    MangadexChapterId = Input.MangadexChapterId,
                    ChapterNumber = Input.ChapterNumber,
                    Content = Input.Content,
                    PageCount = Input.PageCount
                };

                await _chapterService.CreateChapterAsync(chapterViewModel);

                TempData["SuccessMessage"] = $"Chapter '{Input.Title}' has been created successfully!";
                return Redirect($"/admin/chapter?mangaId={Input.MangaId}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the chapter: {ex.Message}";
                var mangas = await _mangaService.GetAllMangaAsync();
                MangaList = new SelectList(mangas, "Id", "Title", Input.MangaId);
                return Page();
            }
        }
    }

    public class CreateChapterInputModel
    {
        [Required(ErrorMessage = "Manga is required")]
        [Display(Name = "Manga")]
        public int MangaId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "MangaDex Chapter ID is required")]
        [StringLength(100, ErrorMessage = "MangaDex Chapter ID cannot exceed 100 characters")]
        [Display(Name = "MangaDex Chapter ID")]
        public string MangadexChapterId { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Chapter number cannot exceed 50 characters")]
        [Display(Name = "Chapter Number")]
        public string? ChapterNumber { get; set; }

        [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
        public string? Content { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Page count must be a positive number")]
        [Display(Name = "Page Count")]
        public int? PageCount { get; set; }
    }
}

