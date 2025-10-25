using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
namespace PRN_MANGA_PROJECT
{
    public class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            // Nếu đã có dữ liệu thì bỏ qua
            if (context.Mangas.Any() || context.Tags.Any())
                return;

            // ====== TAGS ======
            var tags = new List<Tag>
            {
                new Tag { Name = "Action", Description = "Truyện hành động, chiến đấu", Color = "#FF5733", IsActive = true },
                new Tag { Name = "Romance", Description = "Truyện tình cảm lãng mạn", Color = "#E83E8C", IsActive = true },
                new Tag { Name = "Fantasy", Description = "Thế giới phép thuật, phiêu lưu", Color = "#6F42C1", IsActive = true },
                new Tag { Name = "Comedy", Description = "Truyện hài hước, vui vẻ", Color = "#20C997", IsActive = true },
                new Tag { Name = "Horror", Description = "Truyện kinh dị, rùng rợn", Color = "#DC3545", IsActive = false }
            };
            context.Tags.AddRange(tags);
            context.SaveChanges();

            // ====== MANGAS ======
            var mangas = new List<Manga>
            {
                new Manga {
                    Title = "Naruto",
                    MangaDexId = "naruto-001",
                    Author = "Masashi Kishimoto",
                    Artist = "Masashi Kishimoto",
                    Status = "Completed",
                    Description = "Câu chuyện về Naruto, một ninja với ước mơ trở thành Hokage.",
                    CoverImageUrl = "https://cdn.myanimelist.net/images/manga/3/117681.jpg",
                    IsActive = true
                },
                new Manga {
                    Title = "One Piece",
                    MangaDexId = "onepiece-001",
                    Author = "Eiichiro Oda",
                    Artist = "Eiichiro Oda",
                    Status = "Ongoing",
                    Description = "Luffy và băng Mũ Rơm trên hành trình tìm kiếm kho báu huyền thoại One Piece.",
                    CoverImageUrl = "https://cdn.myanimelist.net/images/manga/2/253146.jpg",
                    IsActive = true
                },
                new Manga {
                    Title = "Attack on Titan",
                    MangaDexId = "aot-001",
                    Author = "Hajime Isayama",
                    Artist = "Hajime Isayama",
                    Status = "Completed",
                    Description = "Con người chống lại Titan để giành lại tự do.",
                    CoverImageUrl = "https://cdn.myanimelist.net/images/manga/3/267505.jpg",
                    IsActive = true
                },
                new Manga {
                    Title = "Komi-san wa Komyushou desu",
                    MangaDexId = "komisan-001",
                    Author = "Tomohito Oda",
                    Artist = "Tomohito Oda",
                    Status = "Ongoing",
                    Description = "Cô gái mắc chứng khó giao tiếp cố gắng kết bạn.",
                    CoverImageUrl = "https://cdn.myanimelist.net/images/manga/3/211525.jpg",
                    IsActive = true
                }
            };
            context.Mangas.AddRange(mangas);
            context.SaveChanges();

            // ====== MANGA - TAG RELATIONSHIP ======
            var mangaTags = new List<MangaTag>
            {
                new MangaTag { MangaId = mangas[0].Id, TagId = tags[0].Id }, // Naruto - Action
                new MangaTag { MangaId = mangas[0].Id, TagId = tags[2].Id }, // Naruto - Fantasy

                new MangaTag { MangaId = mangas[1].Id, TagId = tags[0].Id }, // One Piece - Action
                new MangaTag { MangaId = mangas[1].Id, TagId = tags[2].Id }, // One Piece - Fantasy

                new MangaTag { MangaId = mangas[2].Id, TagId = tags[0].Id }, // AOT - Action
                new MangaTag { MangaId = mangas[2].Id, TagId = tags[4].Id }, // AOT - Horror

                new MangaTag { MangaId = mangas[3].Id, TagId = tags[1].Id }, // Komi - Romance
                new MangaTag { MangaId = mangas[3].Id, TagId = tags[3].Id }  // Komi - Comedy
            };

            context.MangaTags.AddRange(mangaTags);
            context.SaveChanges();
        }
    }
}
