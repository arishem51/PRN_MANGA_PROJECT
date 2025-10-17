using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PRN_MANGA_PROJECT.Pages.Public
{
    public class HomePageModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public HomePageModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MangaDexClient");
        }

        public List<MangaItem> MangaList { get; set; } = new();

        public async Task OnGetAsync()
        {

            var response = await _httpClient.GetAsync("manga?limit=6&includes[]=cover_art");
            Console.WriteLine($"Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Request failed");
                return;
            }
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response JSON: {json.Substring(0, 500)}"); // chỉ in 500 ký tự đầu

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<MangaDexResponse>(json, options);

            if (result?.Data == null)
            {
                Console.WriteLine("⚠️ result.Data == null");
                return;
            }
            if (result?.Data != null)
            {
                MangaList = result.Data.Select(m =>
                {
                    string mangaId = m.Id;
                    string title = m.Attributes?.Title?.GetValueOrDefault("en") ?? "No title";
                    string desc = m.Attributes?.Description?.GetValueOrDefault("en") ?? "";
                    string fileName = m.Relationships?
                        .FirstOrDefault(r => r.Type == "cover_art")?
                        .Attributes?.FileName ?? "";

                    string coverUrl = !string.IsNullOrEmpty(fileName)
                        ? $"https://uploads.mangadex.org/covers/{mangaId}/{fileName}"
                        : "/img/no-cover.png";

                    return new MangaItem
                    {
                        Id = mangaId,
                        Title = title,
                        Description = desc,
                        CoverUrl = coverUrl
                    };
                }).ToList();
                Console.WriteLine($"✅ MangaList.Count = {MangaList.Count}");
            }
        }

        // ======= Models =======
        public class MangaDexResponse
        {
            public List<MangaData> Data { get; set; } = new();
        }

        public class MangaData
        {
            public string Id { get; set; } = "";
            public MangaAttributes Attributes { get; set; } = new();
            public List<MangaRelationship> Relationships { get; set; } = new();
        }

        public class MangaAttributes
        {
            public Dictionary<string, string> Title { get; set; } = new();
            public Dictionary<string, string> Description { get; set; } = new();
        }

        public class MangaRelationship
        {
            public string Type { get; set; } = "";
            public CoverAttributes Attributes { get; set; } = new();
        }

        public class CoverAttributes
        {
            public string FileName { get; set; } = "";
        }

        public class MangaItem
        {
            public string Id { get; set; } = "";
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string CoverUrl { get; set; } = "";
        }
    }
}
