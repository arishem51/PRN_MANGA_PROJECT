using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Repositories;
using PRN_MANGA_PROJECT.Services;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Pages.Public
{
    public class HomePageModel : PageModel
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly ITagService _tagService;

        public HomePageModel(IMangaRepository mangaRepository, ITagService tagService)
        {
            _mangaRepository = mangaRepository;
            _tagService = tagService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TagId { get; set; }

        public IEnumerable<PRN_MANGA_PROJECT.Models.Entities.Manga> Mangas { get; set; } = new List<PRN_MANGA_PROJECT.Models.Entities.Manga>();

        public IEnumerable<TagViewModel> Tags { get; set; } = new List<TagViewModel>();

        public async Task OnGetAsync()
        {
            // Load danh sách tag để hiện popup
            Tags = await _tagService.GetActiveTagsAsync();

            // Lọc theo tag
            if (TagId.HasValue)
            {
                var allManga = await _mangaRepository.GetMangaWithTagsAsync();
                Mangas = allManga.Where(m => m.MangaTags.Any(mt => mt.TagId == TagId));
                return;
            }

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Mangas = await _mangaRepository.SearchMangaAsync(SearchTerm);
            }
            else
            {
                Mangas = await _mangaRepository.GetRecentMangaAsync(10);
            }
        }
    }
}
