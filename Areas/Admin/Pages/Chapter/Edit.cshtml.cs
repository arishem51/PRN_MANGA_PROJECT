using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Areas.Admin.Pages.Chapter
{
    public class EditModel : PageModel
    {
        private readonly IChapterService _chapterService;
        private readonly IMangaService _mangaService;

        public EditModel(IChapterService chapterService, IMangaService mangaService)
        {
            _chapterService = chapterService;
            _mangaService = mangaService;
        }

        [BindProperty]
        public EditChapterInputModel Input { get; set; } = new();

        public ChapterViewModel? Chapter { get; set; }
        public MangaViewModel? Manga { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Chapter = await _chapterService.GetChapterWithImagesForAdminAsync(id);
            if (Chapter == null)
            {
                return NotFound();
            }

            Manga = await _mangaService.GetMangaByIdForAdminAsync(Chapter.MangaId);

            Input = new EditChapterInputModel
            {
                Id = Chapter.Id,
                Title = Chapter.Title,
                MangadexChapterId = Chapter.MangadexChapterId,
                ChapterNumber = Chapter.ChapterNumber,
                Content = Chapter.Content,
                PageCount = Chapter.PageCount
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                Chapter = await _chapterService.GetChapterWithImagesForAdminAsync(id);
                if (Chapter != null)
                {
                    Manga = await _mangaService.GetMangaByIdForAdminAsync(Chapter.MangaId);
                }
                return Page();
            }

            if (id != Input.Id)
            {
                return BadRequest();
            }

            try
            {
                var updateViewModel = new UpdateChapterViewModel
                {
                    Title = Input.Title,
                    MangadexChapterId = Input.MangadexChapterId,
                    ChapterNumber = Input.ChapterNumber,
                    Content = Input.Content,
                    PageCount = Input.PageCount
                };

                var updated = await _chapterService.UpdateChapterAsync(id, updateViewModel);

                TempData["SuccessMessage"] = $"Chapter '{updated.Title}' was updated successfully!";
                return Redirect($"/admin/chapter?mangaId={updated.MangaId}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the chapter: {ex.Message}";
                Chapter = await _chapterService.GetChapterWithImagesForAdminAsync(id);
                if (Chapter != null)
                {
                    Manga = await _mangaService.GetMangaByIdForAdminAsync(Chapter.MangaId);
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRefreshImagesAsync(int id)
        {
            try
            {
                var success = await _chapterService.RefreshChapterImagesAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Chapter images have been refreshed successfully from MangaDex!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to refresh images. Please check if the chapter has a valid MangaDex Chapter ID.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while refreshing images: {ex.Message}";
            }

            return RedirectToPage(new { id = id });
        }
    }

    public class EditChapterInputModel
    {
        public int Id { get; set; }

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

