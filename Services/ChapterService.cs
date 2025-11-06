using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Repositories;

namespace PRN_MANGA_PROJECT.Services
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepository;

        public ChapterService(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        public async Task<IEnumerable<ChapterViewModel>> GetAllChaptersAsync()
        {
            var chapters = await _chapterRepository.GetAllAsync();
            return chapters.Select(MapToViewModel);
        }

        public async Task<ChapterViewModel?> GetChapterByIdAsync(int id)
        {
            var chapter = await _chapterRepository.GetByIdAsync(id);
            return chapter != null ? MapToViewModel(chapter) : null;
        }

        public async Task<IEnumerable<ChapterViewModel>> GetChaptersByMangaIdAsync(int mangaId)
        {
            var chapters = await _chapterRepository.GetChaptersByMangaIdAsync(mangaId);
            return chapters.Select(MapToViewModel);
        }

        public async Task<PagedResult<ChapterViewModel>> GetChaptersByMangaIdPagedAsync(int mangaId, PaginationParams paginationParams)
        {
            var pagedResult = await _chapterRepository.GetChaptersByMangaIdPagedAsync(mangaId, paginationParams);
            
            return new PagedResult<ChapterViewModel>
            {
                Data = pagedResult.Data.Select(MapToViewModel).ToList(),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<ChapterViewModel?> GetChapterWithImagesAsync(int id)
        {
            var chapter = await _chapterRepository.GetChapterWithImagesAsync(id);
            return chapter != null ? MapToViewModelWithImages(chapter) : null;
        }

        public async Task<ChapterViewModel?> GetNextChapterAsync(int mangaId, int currentChapterId)
        {
            var chapter = await _chapterRepository.GetNextChapterAsync(mangaId, currentChapterId);
            return chapter != null ? MapToViewModel(chapter) : null;
        }

        public async Task<ChapterViewModel?> GetPreviousChapterAsync(int mangaId, int currentChapterId)
        {
            var chapter = await _chapterRepository.GetPreviousChapterAsync(mangaId, currentChapterId);
            return chapter != null ? MapToViewModel(chapter) : null;
        }

        public async Task<ChapterViewModel> CreateChapterAsync(CreateChapterViewModel model)
        {
            var chapter = new Chapter
            {
                MangaId = model.MangaId,
                Title = model.Title,
                MangadexChapterId = model.MangadexChapterId,
                ChapterNumber = model.ChapterNumber,
                Content = model.Content,
                PageCount = model.PageCount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdChapter = await _chapterRepository.AddAsync(chapter);
            return MapToViewModel(createdChapter);
        }

        public async Task<ChapterViewModel> UpdateChapterAsync(int id, UpdateChapterViewModel model)
        {
            var chapter = await _chapterRepository.GetByIdAsync(id);
            if (chapter == null)
            {
                throw new ArgumentException($"Chapter with ID {id} not found.");
            }

            chapter.Title = model.Title;
            chapter.MangadexChapterId = model.MangadexChapterId;
            chapter.ChapterNumber = model.ChapterNumber;
            chapter.Content = model.Content;
            chapter.PageCount = model.PageCount;
            chapter.UpdatedAt = DateTime.UtcNow;

            await _chapterRepository.UpdateAsync(chapter);
            return MapToViewModel(chapter);
        }

        public async Task<bool> DeleteChapterAsync(int id)
        {
            var chapter = await _chapterRepository.GetByIdAsync(id);
            if (chapter == null)
            {
                return false;
            }

            // Soft delete
            chapter.IsActive = false;
            chapter.UpdatedAt = DateTime.UtcNow;
            await _chapterRepository.UpdateAsync(chapter);
            return true;
        }

        public async Task<bool> ChapterExistsAsync(int id)
        {
            var chapter = await _chapterRepository.GetByIdAsync(id);
            return chapter != null && chapter.IsActive;
        }

        private static ChapterViewModel MapToViewModel(Chapter chapter)
        {
            return new ChapterViewModel
            {
                Id = chapter.Id,
                MangaId = chapter.MangaId,
                Title = chapter.Title,
                MangadexChapterId = chapter.MangadexChapterId,
                ChapterNumber = chapter.ChapterNumber,
                Content = chapter.Content,
                PageCount = chapter.PageCount,
                CreatedAt = chapter.CreatedAt,
                UpdatedAt = chapter.UpdatedAt,
                ImageCount = chapter.ChapterImages?.Count ?? 0
            };
        }

        private static ChapterViewModel MapToViewModelWithImages(Chapter chapter)
        {
            var viewModel = MapToViewModel(chapter);
            viewModel.ChapterImages = chapter.ChapterImages?.Select(ci => new ChapterImageViewModel
            {
                Id = ci.Id,
                ChapterId = ci.ChapterId,
                ImageUrl = ci.ImageUrl,
                PageNumber = ci.PageNumber
            }).ToList() ?? new List<ChapterImageViewModel>();
            return viewModel;
        }
    }
}
