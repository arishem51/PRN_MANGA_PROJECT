
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories;

namespace PRN_MANGA_PROJECT.Pages.Public
{
    public class HomePageModel : PageModel
    {

        private readonly IMangaRepository _mangaRepository;

        public HomePageModel(IMangaRepository mangaRepository)
        {
            _mangaRepository = mangaRepository;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public IEnumerable<PRN_MANGA_PROJECT.Models.Entities.Manga> Mangas { get; set; } = new List<PRN_MANGA_PROJECT.Models.Entities.Manga>();

        public async Task OnGetAsync()
        {
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
