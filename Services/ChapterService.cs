using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Repositories;
using PRN_MANGA_PROJECT.Data;
using System.Text.Json;

namespace PRN_MANGA_PROJECT.Services
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;

        public ChapterService(IChapterRepository chapterRepository, IHttpClientFactory httpClientFactory, ApplicationDbContext context)
        {
            _chapterRepository = chapterRepository;
            _httpClientFactory = httpClientFactory;
            _context = context;
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

        public async Task<ChapterViewModel?> GetChapterWithImagesForAdminAsync(int id)
        {
            var chapter = await _chapterRepository.GetChapterWithImagesForAdminAsync(id);
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

            // Fetch and store chapter images from MangaDex
            try
            {
                await FetchAndStoreChapterImagesAsync(createdChapter.Id, model.MangadexChapterId);
            }
            catch (Exception ex)
            {
                // Log error but don't fail chapter creation
                // You might want to add proper logging here
                Console.WriteLine($"Error fetching images for chapter {createdChapter.Id}: {ex.Message}");
            }

            return MapToViewModel(createdChapter);
        }

        private async Task FetchAndStoreChapterImagesAsync(int chapterId, string mangadexChapterId)
        {
            var httpClient = _httpClientFactory.CreateClient("MangaDexClient");
            var response = await httpClient.GetAsync($"at-home/server/{mangadexChapterId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch images from MangaDex: {response.StatusCode}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<MangaDexAtHomeResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null || string.IsNullOrEmpty(apiResponse.BaseUrl) || apiResponse.Chapter == null)
            {
                throw new InvalidOperationException("Invalid response from MangaDex API");
            }

            var baseUrl = apiResponse.BaseUrl;
            var hash = apiResponse.Chapter.Hash;
            var imageFilenames = apiResponse.Chapter.Data ?? apiResponse.Chapter.DataSaver ?? new List<string>();

            if (!imageFilenames.Any())
            {
                return; // No images to store
            }

            var chapterImages = new List<ChapterImage>();
            for (int i = 0; i < imageFilenames.Count; i++)
            {
                var imageUrl = $"{baseUrl}/data/{hash}/{imageFilenames[i]}";
                chapterImages.Add(new ChapterImage
                {
                    ChapterId = chapterId,
                    ImageUrl = imageUrl,
                    PageNumber = i + 1,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.ChapterImages.AddRange(chapterImages);
            await _context.SaveChangesAsync();

            // Update chapter page count if not set
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter != null && !chapter.PageCount.HasValue)
            {
                chapter.PageCount = imageFilenames.Count;
                await _chapterRepository.UpdateAsync(chapter);
            }
        }

        private class MangaDexAtHomeResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("result")]
            public string Result { get; set; } = string.Empty;
            
            [System.Text.Json.Serialization.JsonPropertyName("baseUrl")]
            public string? BaseUrl { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("chapter")]
            public MangaDexChapterData? Chapter { get; set; }
        }

        private class MangaDexChapterData
        {
            [System.Text.Json.Serialization.JsonPropertyName("hash")]
            public string Hash { get; set; } = string.Empty;
            
            [System.Text.Json.Serialization.JsonPropertyName("data")]
            public List<string>? Data { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("dataSaver")]
            public List<string>? DataSaver { get; set; }
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
