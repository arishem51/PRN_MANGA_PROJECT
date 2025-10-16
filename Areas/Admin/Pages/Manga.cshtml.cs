using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;

namespace PRN_MANGA_PROJECT.Areas.Admin.Pages
{
    public class MangaModel : PageModel
    {
        private readonly IMangaService _mangaService;

        public MangaModel(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }

        public IList<MangaViewModel> Mangas { get; private set; } = new List<MangaViewModel>();

        public async Task OnGetAsync()
        {
            var mangas = await _mangaService.GetAllMangaAsync();
            Mangas = mangas.ToList();
        }
    }
}


