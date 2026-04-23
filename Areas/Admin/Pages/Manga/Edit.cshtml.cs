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
        private readonly IChapterService _chapterService;
        private readonly ITagService _tagService;

        public EditModel(IMangaService mangaService, IChapterService chapterService, ITagService tagService)
        {
            _mangaService = mangaService;
            _chapterService = chapterService;
            _tagService = tagService;
        }

        [BindProperty]
        public EditInputModel Input { get; set; } = new();

        public PagedResult<ChapterViewModel> ChaptersPaged { get; set; } = new();
        
        public List<TagViewModel> AvailableTags { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int ChapterPage { get; set; } = 1;

        private const int ChapterPageSize = 10;

        public async Task<IActionResult> OnGetAsync(int id, int chapterPage = 1)
        {
            var manga = await _mangaService.GetMangaByIdForAdminAsync(id);
            if (manga == null)
            {
                return NotFound();
            }

            // Load all available tags
            AvailableTags = (await _tagService.GetActiveTagsAsync()).ToList();

            Input = new EditInputModel
            {
                Id = manga.Id,
                Title = manga.Title,
                Author = manga.Author,
                Artist = manga.Artist,
                Description = manga.Description,
                Status = manga.Status,
                CoverImageUrl = manga.CoverImageUrl,
                SelectedTagIds = manga.Tags?.Select(t => t.Id).ToList() ?? new List<int>()
            };

            // Load chapters for this manga with pagination
            ChapterPage = chapterPage;
            var paginationParams = new PaginationParams
            {
                Page = chapterPage,
                PageSize = ChapterPageSize
            };
            ChaptersPaged = await _chapterService.GetChaptersByMangaIdPagedAsync(id, paginationParams);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                // Reload tags and chapters if validation fails
                AvailableTags = (await _tagService.GetActiveTagsAsync()).ToList();
                var paginationParams = new PaginationParams
                {
                    Page = ChapterPage,
                    PageSize = ChapterPageSize
                };
                ChaptersPaged = await _chapterService.GetChaptersByMangaIdPagedAsync(id, paginationParams);
                return Page();
            }

            if (id != Input.Id)
            {
                return BadRequest();
            }

            // Convert selected tag IDs to TagViewModels
            var selectedTags = Input.SelectedTagIds?
                .Select(tagId => new TagViewModel { Id = tagId })
                .ToList() ?? new List<TagViewModel>();

            var updated = await _mangaService.UpdateMangaAsync(new MangaViewModel
            {
                Id = Input.Id,
                Title = Input.Title,
                Author = Input.Author,
                Artist = Input.Artist,
                Description = Input.Description,
                Status = Input.Status,
                CoverImageUrl = Input.CoverImageUrl,
                Tags = selectedTags
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

        public List<int> SelectedTagIds { get; set; } = new List<int>();
    }
}


