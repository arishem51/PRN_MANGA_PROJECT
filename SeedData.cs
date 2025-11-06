using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT
{
    public class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context, IHttpClientFactory clientFactory)
        {
            await context.Database.MigrateAsync();

            if (context.Mangas.Any())
            {
                Console.WriteLine("⚠️ Dữ liệu đã tồn tại, bỏ qua seeding.");
                return;
            }

            var random = new Random();

            // ====== TAGS ======
            var tags = new List<Tag>
            {
                new Tag { Name = "Action", Description = "Truyện hành động, chiến đấu", Color = "#FF5733", IsActive = true },
                new Tag { Name = "Romance", Description = "Truyện tình cảm lãng mạn", Color = "#E83E8C", IsActive = true },
                new Tag { Name = "Fantasy", Description = "Thế giới phép thuật, phiêu lưu", Color = "#6F42C1", IsActive = true },
                new Tag { Name = "Comedy", Description = "Truyện hài hước, vui vẻ", Color = "#20C997", IsActive = true },
                new Tag { Name = "Horror", Description = "Truyện kinh dị, rùng rợn", Color = "#DC3545", IsActive = true },
                new Tag { Name = "Drama", Description = "Tình tiết cảm động, sâu sắc", Color = "#17A2B8", IsActive = true },
                new Tag { Name = "Sci-Fi", Description = "Khoa học viễn tưởng", Color = "#6610F2", IsActive = true },
                new Tag { Name = "Slice of Life", Description = "Cuộc sống đời thường", Color = "#FD7E14", IsActive = true }
            };
            context.Tags.AddRange(tags);
            await context.SaveChangesAsync();

            // ====== MANGAS ======
            var client = clientFactory.CreateClient("MangaDexClient");

            var sampleTitles = new[]
            {
                "Naruto", "Bleach", "One Piece", "Attack on Titan", "My Hero Academia",
                "Demon Slayer", "Jujutsu Kaisen", "Tokyo Ghoul", "Death Note", "Fairy Tail"
            };

            foreach (var title in sampleTitles)
            {
                try
                {
                    var url = $"manga?title={Uri.EscapeDataString(title)}&includes[]=cover_art&limit=1";
                    var resp = await client.GetFromJsonAsync<MangaDexResponse>(url);

                    string coverUrl = $"https://picsum.photos/seed/{title.Replace(" ", "_")}/400/600";
                    List<string> apiTags = new();

                    if (resp?.data != null && resp.data.Count > 0)
                    {
                        var mangaData = resp.data.First();

                        // Lấy ảnh thật
                        var cover = mangaData.relationships?.FirstOrDefault(r => r.type == "cover_art");
                        if (cover?.attributes?.fileName != null)
                        {
                            coverUrl = $"https://uploads.mangadex.org/covers/{mangaData.id}/{cover.attributes.fileName}";
                        }

                        // Lấy tag thật từ MangaDex
                        apiTags = mangaData.attributes?.tags?
                            .Select(t => t.attributes?.name?["en"] ?? "")
                            .Where(n => !string.IsNullOrWhiteSpace(n))
                            .ToList() ?? new List<string>();

                        Console.WriteLine($"✅ {title} → {apiTags.Count} tags, cover: {coverUrl}");
                    }

                    // ✅ Thêm Manga vào DB trước
                    var manga = new Manga
                    {
                        Title = title,
                        Author = $"{title} Author",
                        Artist = $"{title} Artist",
                        Status = random.Next(0, 2) == 0 ? "Ongoing" : "Completed",
                        Description = $"Câu chuyện hấp dẫn về {title}, nơi nhân vật chính phải vượt qua thử thách.",
                        CoverImageUrl = coverUrl,
                        IsActive = true
                    };

                    context.Mangas.Add(manga);
                    await context.SaveChangesAsync(); // cần ID để gắn tag

                    // ✅ GẮN TAG
                    if (apiTags.Any())
                    {
                        foreach (var tagName in apiTags)
                        {
                            var match = tags.FirstOrDefault(t =>
                                tagName.Contains(t.Name, StringComparison.OrdinalIgnoreCase));
                            if (match != null)
                            {
                                context.MangaTags.Add(new MangaTag
                                {
                                    MangaId = manga.Id,
                                    TagId = match.Id
                                });
                            }
                        }
                    }
                    else
                    {
                        // fallback random tag
                        var randomTags = tags.OrderBy(x => random.Next()).Take(random.Next(2, 4));
                        foreach (var tag in randomTags)
                        {
                            context.MangaTags.Add(new MangaTag
                            {
                                MangaId = manga.Id,
                                TagId = tag.Id
                            });
                        }
                    }

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi khi seed {title}: {ex.Message}");
                }
            }

            // ====== CHAPTERS ======
            var chapters = new List<Chapter>();
            var mangas = await context.Mangas.ToListAsync();

            foreach (var manga in mangas)
            {
                for (int i = 1; i <= 3; i++)
                {
                    chapters.Add(new Chapter
                    {
                        MangaId = manga.Id,
                        Title = $"{manga.Title} - Chapter {i}",
                        ChapterNumber = i.ToString(),
                        Content = $"Nội dung tóm tắt của {manga.Title} chương {i}.",
                        PageCount = 3,
                        IsActive = true
                    });
                }
            }
            context.Chapters.AddRange(chapters);
            await context.SaveChangesAsync();

            // ====== CHAPTER IMAGES ======
            var chapterImages = new List<ChapterImage>();
            foreach (var chapter in chapters)
            {
                for (int p = 1; p <= 3; p++)
                {
                    chapterImages.Add(new ChapterImage
                    {
                        ChapterId = chapter.Id,
                        ImageUrl = $"https://picsum.photos/seed/{chapter.Title.Replace(" ", "_")}_p{p}/800/1200",
                        PageNumber = p
                    });
                }
            }
            context.ChapterImages.AddRange(chapterImages);
            await context.SaveChangesAsync();

            Console.WriteLine("🎉 Seeding hoàn tất với ảnh thật từ MangaDex và gắn tag thành công!");
        }

        // ====== DTOs cho API MangaDex ======
        private class MangaDexResponse
        {
            public List<MangaData> data { get; set; } = new();
        }

        private class MangaData
        {
            public string id { get; set; } = "";
            public MangaAttributes attributes { get; set; } = new();
            public List<MangaRelationship> relationships { get; set; } = new();
        }

        private class MangaAttributes
        {
            public List<MangaTagDTO> tags { get; set; } = new();
        }

        private class MangaTagDTO
        {
            public string id { get; set; } = "";
            public TagAttr attributes { get; set; } = new();
        }

        private class TagAttr
        {
            public Dictionary<string, string> name { get; set; } = new();
        }

        private class MangaRelationship
        {
            public string type { get; set; } = "";
            public RelationshipAttributes attributes { get; set; } = new();
        }

        private class RelationshipAttributes
        {
            public string? fileName { get; set; }
        }
    }
}
