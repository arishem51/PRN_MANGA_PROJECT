using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Repositories;
using PRN_MANGA_PROJECT.Data;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace PRN_MANGA_PROJECT.Services
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChapterService> _logger;

        public ChapterService(IChapterRepository chapterRepository, IHttpClientFactory httpClientFactory, ApplicationDbContext context, ILogger<ChapterService> logger)
        {
            _chapterRepository = chapterRepository;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
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

            // Always fetch images from MangaDex API if MangaDex Chapter ID is valid
            _logger.LogInformation("=== Checking if images need to be fetched for new chapter {ChapterId} ===", createdChapter.Id);
            _logger.LogInformation("MangaDex Chapter ID value: '{MangadexChapterId}' (IsNullOrEmpty: {IsEmpty})", 
                model.MangadexChapterId ?? "NULL", string.IsNullOrEmpty(model.MangadexChapterId));
            
            if (!string.IsNullOrWhiteSpace(model.MangadexChapterId))
            {
                _logger.LogInformation("Fetching images for newly created chapter {ChapterId} from MangaDex API. MangaDex ID: {MangadexChapterId}", createdChapter.Id, model.MangadexChapterId);
                try
                {
                    await FetchAndStoreChapterImagesAsync(createdChapter.Id, model.MangadexChapterId);
                    _logger.LogInformation("Successfully fetched and stored images for chapter {ChapterId}", createdChapter.Id);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail chapter creation
                    _logger.LogError(ex, "Error fetching images for chapter {ChapterId} with MangaDex ID {MangadexChapterId}: {ErrorMessage}", createdChapter.Id, model.MangadexChapterId, ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Skipping image fetch for chapter {ChapterId} because MangaDex Chapter ID is empty or null", createdChapter.Id);
            }

            return MapToViewModel(createdChapter);
        }

        private async Task FetchAndStoreChapterImagesAsync(int chapterId, string mangadexChapterId)
        {
            _logger.LogInformation("=== FetchAndStoreChapterImagesAsync CALLED ===");
            _logger.LogInformation("Starting to fetch images for chapter {ChapterId} from MangaDex API (Chapter ID: {MangadexChapterId})", chapterId, mangadexChapterId);
            
            if (string.IsNullOrWhiteSpace(mangadexChapterId))
            {
                _logger.LogWarning("MangaDex Chapter ID is empty or null for chapter {ChapterId}. Cannot fetch images.", chapterId);
                return;
            }

            var httpClient = _httpClientFactory.CreateClient("MangaDexClient");
            var response = await httpClient.GetAsync($"at-home/server/{mangadexChapterId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch images from MangaDex API for chapter {ChapterId}. Status Code: {StatusCode}", chapterId, response.StatusCode);
                throw new HttpRequestException($"Failed to fetch images from MangaDex: {response.StatusCode}");
            }

            _logger.LogInformation("Successfully received response from MangaDex API for chapter {ChapterId}", chapterId);

            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw JSON response from MangaDex API for chapter {ChapterId} (first 500 chars): {JsonPreview}", 
                chapterId, jsonContent.Length > 500 ? jsonContent.Substring(0, 500) + "..." : jsonContent);
            
            var apiResponse = JsonSerializer.Deserialize<MangaDexAtHomeResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Deserialized API response for chapter {ChapterId}: Result='{Result}', BaseUrl='{BaseUrl}', Chapter is null: {ChapterIsNull}", 
                chapterId, 
                apiResponse?.Result ?? "NULL", 
                apiResponse?.BaseUrl ?? "NULL",
                apiResponse?.Chapter == null);

            if (apiResponse == null)
            {
                _logger.LogError("Invalid response from MangaDex API for chapter {ChapterId}. API response is null. Full JSON: {JsonContent}", chapterId, jsonContent);
                throw new InvalidOperationException("Invalid response from MangaDex API: Response is null");
            }

            if (string.IsNullOrEmpty(apiResponse.BaseUrl))
            {
                _logger.LogError("Invalid response from MangaDex API for chapter {ChapterId}. BaseUrl is null or empty. Full JSON: {JsonContent}", chapterId, jsonContent);
                throw new InvalidOperationException("Invalid response from MangaDex API: BaseUrl is missing");
            }

            if (apiResponse.Chapter == null)
            {
                _logger.LogError("Invalid response from MangaDex API for chapter {ChapterId}. Chapter object is null. Full JSON: {JsonContent}", chapterId, jsonContent);
                throw new InvalidOperationException("Invalid response from MangaDex API: Chapter object is missing");
            }

            if (string.IsNullOrEmpty(apiResponse.Chapter.Hash))
            {
                _logger.LogError("Invalid response from MangaDex API for chapter {ChapterId}. Chapter.Hash is null or empty. Full JSON: {JsonContent}", chapterId, jsonContent);
                throw new InvalidOperationException("Invalid response from MangaDex API: Chapter hash is missing");
            }

            var baseUrl = apiResponse.BaseUrl;
            var hash = apiResponse.Chapter.Hash;
            var imageFilenames = apiResponse.Chapter.Data ?? apiResponse.Chapter.DataSaver ?? new List<string>();

            _logger.LogInformation("Parsed MangaDex API response for chapter {ChapterId}. Found {ImageCount} images. BaseUrl: {BaseUrl}", chapterId, imageFilenames.Count, baseUrl);

            // Delete existing images for this chapter (override/replace)
            var existingImages = _context.ChapterImages.Where(ci => ci.ChapterId == chapterId).ToList();
            if (existingImages.Any())
            {
                _logger.LogInformation("Deleting {ExistingImageCount} existing images for chapter {ChapterId} before adding new ones", existingImages.Count, chapterId);
                _context.ChapterImages.RemoveRange(existingImages);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted existing images for chapter {ChapterId}", chapterId);
            }

            if (!imageFilenames.Any())
            {
                _logger.LogWarning("No images found in MangaDex API response for chapter {ChapterId}", chapterId);
                return; // No images to store
            }

            var chapterImages = new List<ChapterImage>();
            var imageUrls = new List<string>();
            for (int i = 0; i < imageFilenames.Count; i++)
            {
                var imageUrl = $"{baseUrl}/data/{hash}/{imageFilenames[i]}";
                imageUrls.Add(imageUrl);
                chapterImages.Add(new ChapterImage
                {
                    ChapterId = chapterId,
                    ImageUrl = imageUrl,
                    PageNumber = i + 1,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _logger.LogInformation("Prepared {ImageCount} image URLs for chapter {ChapterId}. Saving to database...", chapterImages.Count, chapterId);
            _logger.LogInformation("All MangaDex image URLs for chapter {ChapterId}: {ImageUrls}", chapterId, string.Join(" | ", imageUrls));

            _context.ChapterImages.AddRange(chapterImages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully saved {ImageCount} images to database for chapter {ChapterId}", chapterImages.Count, chapterId);
            _logger.LogInformation("Saved MangaDex image URLs for chapter {ChapterId}: {ImageUrls}", chapterId, string.Join(" | ", imageUrls));

            // Update chapter page count if not set
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter != null && !chapter.PageCount.HasValue)
            {
                chapter.PageCount = imageFilenames.Count;
                await _chapterRepository.UpdateAsync(chapter);
                _logger.LogInformation("Updated page count to {PageCount} for chapter {ChapterId}", imageFilenames.Count, chapterId);
            }

            _logger.LogInformation("Successfully completed fetching and storing images for chapter {ChapterId}", chapterId);
        }

        private class MangaDexAtHomeResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("result")]
            public string Result { get; set; } = string.Empty;
            
            [System.Text.Json.Serialization.JsonPropertyName("baseUrl")]
            public string BaseUrl { get; set; } = string.Empty;
            
            [System.Text.Json.Serialization.JsonPropertyName("chapter")]
            public MangaDexChapterData Chapter { get; set; } = new();
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

            // Check if MangaDex Chapter ID changed - if so, fetch new images
            bool mangadexIdChanged = chapter.MangadexChapterId != model.MangadexChapterId;

            chapter.Title = model.Title;
            chapter.MangadexChapterId = model.MangadexChapterId ?? string.Empty;
            chapter.ChapterNumber = model.ChapterNumber;
            chapter.Content = model.Content;
            chapter.PageCount = model.PageCount;
            chapter.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Updating chapter {ChapterId} with new information. Chapter URL: /admin/chapter/{ChapterId}/edit", id, id);
            await _chapterRepository.UpdateAsync(chapter);
            _logger.LogInformation("Successfully updated chapter {ChapterId}. Chapter URL: /admin/chapter/{ChapterId}/edit", id, id);

            // Always fetch images if MangaDex Chapter ID is valid
            _logger.LogInformation("=== Checking if images need to be fetched for chapter {ChapterId} ===", id);
            _logger.LogInformation("MangaDex ID changed: {Changed}, New ID: '{NewId}' (IsNullOrEmpty: {IsEmpty})", 
                mangadexIdChanged, model.MangadexChapterId ?? "NULL", string.IsNullOrEmpty(model.MangadexChapterId));
            
            // Check if chapter has existing images
            var existingImageCount = await _context.ChapterImages.CountAsync(ci => ci.ChapterId == id);
            _logger.LogInformation("Chapter {ChapterId} currently has {ImageCount} images in database", id, existingImageCount);
            
            // Always fetch images if MangaDex Chapter ID is valid (not empty)
            if (!string.IsNullOrWhiteSpace(model.MangadexChapterId))
            {
                _logger.LogInformation("Fetching images for chapter {ChapterId} from MangaDex API. MangaDex ID: {MangadexChapterId}", id, model.MangadexChapterId);
                try
                {
                    await FetchAndStoreChapterImagesAsync(id, model.MangadexChapterId);
                    _logger.LogInformation("Successfully fetched and stored new images for chapter {ChapterId} after update", id);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail chapter update
                    _logger.LogError(ex, "Error fetching images for chapter {ChapterId} with MangaDex ID {MangadexChapterId} during update: {ErrorMessage}", 
                        id, model.MangadexChapterId, ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Skipping image fetch for chapter {ChapterId}. MangaDex Chapter ID is empty or null.", id);
            }

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

        public async Task<bool> RefreshChapterImagesAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                _logger.LogWarning("Cannot refresh images for chapter {ChapterId}: Chapter not found", chapterId);
                return false;
            }

            if (string.IsNullOrEmpty(chapter.MangadexChapterId))
            {
                _logger.LogWarning("Cannot refresh images for chapter {ChapterId}: MangaDex Chapter ID is missing", chapterId);
                return false;
            }

            try
            {
                _logger.LogInformation("Manually refreshing images for chapter {ChapterId} with MangaDex ID {MangadexChapterId}", chapterId, chapter.MangadexChapterId);
                await FetchAndStoreChapterImagesAsync(chapterId, chapter.MangadexChapterId);
                _logger.LogInformation("Successfully refreshed images for chapter {ChapterId}", chapterId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing images for chapter {ChapterId} with MangaDex ID {MangadexChapterId}: {ErrorMessage}", 
                    chapterId, chapter.MangadexChapterId, ex.Message);
                return false;
            }
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
