using PRN_MANGA_PROJECT.Models.ViewModels;
using System.Net.Http.Json;

namespace PRN_MANGA_PROJECT.Services
{
    public class ChapterUIService
    {
        private readonly HttpClient _httpClient;

        public ChapterUIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<ChapterViewModel>> GetChaptersByMangaIdAsync(int mangaId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<ChapterViewModel>>($"api/chapter/manga/{mangaId}");
                return response ?? new List<ChapterViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chapters for manga {mangaId}: {ex.Message}");
                return new List<ChapterViewModel>();
            }
        }

        public async Task<PagedResult<ChapterViewModel>> GetChaptersByMangaIdPagedAsync(int mangaId, int page, int pageSize)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<PagedResult<ChapterViewModel>>($"api/chapter/manga/{mangaId}/paged?page={page}&pageSize={pageSize}");
                return response ?? new PagedResult<ChapterViewModel>
                {
                    Data = new List<ChapterViewModel>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching paged chapters for manga {mangaId}: {ex.Message}");
                return new PagedResult<ChapterViewModel>
                {
                    Data = new List<ChapterViewModel>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
        }

        public async Task<ChapterViewModel?> GetChapterWithImagesAsync(int chapterId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ChapterViewModel>($"api/chapter/{chapterId}/with-images");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chapter {chapterId} with images: {ex.Message}");
                return null;
            }
        }

        public async Task<ChapterViewModel?> GetNextChapterAsync(int chapterId, int mangaId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ChapterViewModel>($"api/chapter/{chapterId}/next?mangaId={mangaId}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching next chapter for {chapterId}: {ex.Message}");
                return null;
            }
        }

        public async Task<ChapterViewModel?> GetPreviousChapterAsync(int chapterId, int mangaId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ChapterViewModel>($"api/chapter/{chapterId}/previous?mangaId={mangaId}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching previous chapter for {chapterId}: {ex.Message}");
                return null;
            }
        }

        public async Task<ChapterViewModel?> GetChapterByIdAsync(int chapterId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ChapterViewModel>($"api/chapter/{chapterId}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chapter {chapterId}: {ex.Message}");
                return null;
            }
        }

        public async Task<int?> GetNextChapterIdAsync(int chapterId, int mangaId)
        {
            try
            {
                var nextChapter = await GetNextChapterAsync(chapterId, mangaId);
                return nextChapter?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching next chapter ID for {chapterId}: {ex.Message}");
                return null;
            }
        }

        public async Task<int?> GetPreviousChapterIdAsync(int chapterId, int mangaId)
        {
            try
            {
                var previousChapter = await GetPreviousChapterAsync(chapterId, mangaId);
                return previousChapter?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching previous chapter ID for {chapterId}: {ex.Message}");
                return null;
            }
        }
    }
}
