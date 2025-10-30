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
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 3; 
        public int TotalPages { get; set; }

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
           
            Tags = await _tagService.GetActiveTagsAsync();

            if (TagId.HasValue)
            {
                var allManga = await _mangaRepository.GetMangaWithTagsAsync();
                Mangas = allManga.Where(m => m.MangaTags.Any(mt => mt.TagId == TagId));
                return;
            }

            int totalManga;

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                (Mangas, totalManga) = await _mangaRepository.SearchMangaPagedAsync(SearchTerm, PageNumber, PageSize);
            }
            else
            {
                (Mangas, totalManga) = await _mangaRepository.GetMangaPagedAsync(PageNumber, PageSize);
            }

            TotalPages = (int)Math.Ceiling(totalManga / (double)PageSize);
        }
    }
}
