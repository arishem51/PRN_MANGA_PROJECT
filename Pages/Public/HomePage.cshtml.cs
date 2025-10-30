
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories;

namespace PRN_MANGA_PROJECT.Pages.Public
{
    public class HomePageModel : PageModel
    {

        private readonly IMangaRepository _mangaRepository;
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 3; 
        public int TotalPages { get; set; }

        public HomePageModel(IMangaRepository mangaRepository)
        {
            _mangaRepository = mangaRepository;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public IEnumerable<PRN_MANGA_PROJECT.Models.Entities.Manga> Mangas { get; set; } = new List<PRN_MANGA_PROJECT.Models.Entities.Manga>();

        public async Task OnGetAsync()
        {
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
