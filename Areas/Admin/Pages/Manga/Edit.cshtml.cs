using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Areas.Admin.Pages.Manga
{
    public class EditModel : PageModel
    {
        private readonly IMangaService _mangaService;

        public EditModel(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }

        [BindProperty]
        public EditInputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var manga = await _mangaService.GetMangaByIdAsync(id);
            if (manga == null)
            {
                return NotFound();
            }

            Input = new EditInputModel
            {
                Id = manga.Id,
                Title = manga.Title,
                Author = manga.Author,
                Artist = manga.Artist,
                Description = manga.Description,
                Status = manga.Status,
                CoverImageUrl = manga.CoverImageUrl
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (id != Input.Id)
            {
                return BadRequest();
            }

            var updated = await _mangaService.UpdateMangaAsync(new MangaViewModel
            {
                Id = Input.Id,
                Title = Input.Title,
                Author = Input.Author,
                Artist = Input.Artist,
                Description = Input.Description,
                Status = Input.Status,
                CoverImageUrl = Input.CoverImageUrl
            });

            TempData["SuccessMessage"] = $"Manga '{updated.Title}' was updated.";
            return Redirect("/admin/manga");
        }
    }

    public class EditInputModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;


        [StringLength(100)]
        public string? Author { get; set; }

        [StringLength(100)]
        public string? Artist { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(500)]
        [Url]
        public string? CoverImageUrl { get; set; }
    }
}


