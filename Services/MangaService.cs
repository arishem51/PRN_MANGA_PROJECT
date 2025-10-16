using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Repositories;

namespace PRN_MANGA_PROJECT.Services
{
    public class MangaService : IMangaService
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly IBookmarkRepository _bookmarkRepository;

        public MangaService(IMangaRepository mangaRepository, IBookmarkRepository bookmarkRepository)
        {
            _mangaRepository = mangaRepository;
            _bookmarkRepository = bookmarkRepository;
        }

        public async Task<IEnumerable<MangaViewModel>> GetAllMangaAsync()
        {
            var mangas = await _mangaRepository.GetAllMangaWithTagsAsync();
            return mangas.Select(MapToViewModel);
        }

        public async Task<IEnumerable<MangaViewModel>> GetActiveMangaAsync()
        {
            var mangas = await _mangaRepository.GetMangaWithTagsAsync();
            return mangas.Select(MapToViewModel);
        }

        public async Task<MangaViewModel?> GetMangaByIdAsync(int id)
        {
            var manga = await _mangaRepository.GetMangaWithTagsByIdAsync(id);
            return manga != null ? MapToViewModel(manga) : null;
        }

        public async Task<IEnumerable<MangaViewModel>> SearchMangaAsync(string searchTerm)
        {
            var mangas = await _mangaRepository.SearchMangaAsync(searchTerm);
            return mangas.Select(MapToViewModel);
        }

        public async Task<IEnumerable<MangaViewModel>> GetMangaByTagAsync(int tagId)
        {
            var mangas = await _mangaRepository.GetMangaByTagAsync(tagId);
            return mangas.Select(MapToViewModel);
        }

        public async Task<IEnumerable<MangaViewModel>> GetMangaByStatusAsync(string status)
        {
            var mangas = await _mangaRepository.GetMangaByStatusAsync(status);
            return mangas.Select(MapToViewModel);
        }

        public async Task<IEnumerable<MangaViewModel>> GetPopularMangaAsync(int count)
        {
            var mangas = await _mangaRepository.GetPopularMangaAsync(count);
            return mangas.Select(MapToViewModel);
        }

        public async Task<IEnumerable<MangaViewModel>> GetRecentMangaAsync(int count)
        {
            var mangas = await _mangaRepository.GetRecentMangaAsync(count);
            return mangas.Select(MapToViewModel);
        }

        public async Task<MangaViewModel> CreateMangaAsync(MangaViewModel mangaViewModel)
        {
            var manga = new Manga
            {
                Title = mangaViewModel.Title,
                MangaDexId = mangaViewModel.MangaDexId,
                Author = mangaViewModel.Author,
                Artist = mangaViewModel.Artist,
                Description = mangaViewModel.Description,
                Status = mangaViewModel.Status,
                CoverImageUrl = mangaViewModel.CoverImageUrl
            };

            var createdManga = await _mangaRepository.AddAsync(manga);
            return MapToViewModel(createdManga);
        }

        public async Task<MangaViewModel> UpdateMangaAsync(MangaViewModel mangaViewModel)
        {
            var manga = await _mangaRepository.GetByIdAsync(mangaViewModel.Id);
            if (manga == null)
                throw new ArgumentException("Manga not found");

            manga.Title = mangaViewModel.Title;
            manga.Author = mangaViewModel.Author;
            manga.Artist = mangaViewModel.Artist;
            manga.Description = mangaViewModel.Description;
            manga.Status = mangaViewModel.Status;
            manga.CoverImageUrl = mangaViewModel.CoverImageUrl;
            manga.UpdatedAt = DateTime.UtcNow;

            await _mangaRepository.UpdateAsync(manga);
            return MapToViewModel(manga);
        }

        public async Task DeleteMangaAsync(int id)
        {
            var manga = await _mangaRepository.GetByIdAsync(id);
            if (manga != null)
            {
                manga.IsActive = false;
                await _mangaRepository.UpdateAsync(manga);
            }
        }

        public async Task ActivateMangaAsync(int id)
        {
            var manga = await _mangaRepository.GetByIdAsync(id);
            if (manga != null)
            {
                manga.IsActive = true;
                await _mangaRepository.UpdateAsync(manga);
            }
        }

        public async Task<bool> BookmarkMangaAsync(string userId, int mangaId)
        {
            var existingBookmark = await _bookmarkRepository.FirstOrDefaultAsync(b => b.UserId == userId && b.MangaId == mangaId);
            if (existingBookmark != null)
                return false;

            var bookmark = new Bookmark
            {
                UserId = userId,
                MangaId = mangaId
            };

            await _bookmarkRepository.AddAsync(bookmark);
            return true;
        }

        public async Task<bool> RemoveBookmarkAsync(string userId, int mangaId)
        {
            var bookmark = await _bookmarkRepository.FirstOrDefaultAsync(b => b.UserId == userId && b.MangaId == mangaId);
            if (bookmark == null)
                return false;

            await _bookmarkRepository.DeleteAsync(bookmark);
            return true;
        }

        public async Task<bool> IsBookmarkedAsync(string userId, int mangaId)
        {
            return await _bookmarkRepository.ExistsAsync(b => b.UserId == userId && b.MangaId == mangaId);
        }

        private static MangaViewModel MapToViewModel(Manga manga)
        {
            return new MangaViewModel
            {
                Id = manga.Id,
                Title = manga.Title,
                MangaDexId = manga.MangaDexId,
                Author = manga.Author,
                Artist = manga.Artist,
                Description = manga.Description,
                Status = manga.Status,
                CoverImageUrl = manga.CoverImageUrl,
                CreatedAt = manga.CreatedAt,
                UpdatedAt = manga.UpdatedAt,
                IsActive = manga.IsActive,
                Tags = manga.MangaTags.Select(mt => new TagViewModel
                {
                    Id = mt.Tag.Id,
                    Name = mt.Tag.Name,
                    Description = mt.Tag.Description,
                    Color = mt.Tag.Color
                }).ToList(),
                ChapterCount = manga.Chapters.Count(c => c.IsActive)
            };
        }
    }
}
