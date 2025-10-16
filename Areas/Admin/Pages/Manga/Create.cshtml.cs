using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Areas.Admin.Pages.Manga
{
    public class CreateModel : PageModel
    {
        private readonly IMangaService _mangaService;

        public CreateModel(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }

        [BindProperty]
        public CreateMangaInputModel Input { get; set; } = new();

        public void OnGet()
        {
            // Initialize page data if needed
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var mangaViewModel = new MangaViewModel
                {
                    Title = Input.Title,
                    MangaDexId = Input.MangaDexId,
                    Author = Input.Author,
                    Artist = Input.Artist,
                    Description = Input.Description,
                    Status = Input.Status,
                    CoverImageUrl = Input.CoverImageUrl
                };

                await _mangaService.CreateMangaAsync(mangaViewModel);

                TempData["SuccessMessage"] = $"Manga '{Input.Title}' has been created successfully!";
                return Redirect("/admin/manga");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the manga: {ex.Message}";
                return Page();
            }
        }
    }

    public class CreateMangaInputModel
    {
        [Required(ErrorMessage = "MangaDex ID is required")]
        [StringLength(100, ErrorMessage = "MangaDex ID cannot exceed 100 characters")]
        public string MangaDexId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string? Author { get; set; }

        [StringLength(100, ErrorMessage = "Artist name cannot exceed 100 characters")]
        public string? Artist { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }

        [StringLength(500, ErrorMessage = "Cover image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? CoverImageUrl { get; set; }
    }
}
