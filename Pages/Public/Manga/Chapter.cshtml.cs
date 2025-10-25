using System.Security.Claims;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using System.Security.Claims;
using System.Net.Http;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PRN_MANGA_PROJECT.Pages.Public.Manga
{
    public class ChapterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        [BindProperty]
        public CommentViewModel Input { get; set; } = new CommentViewModel();

        public ChapterModel(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("MangaDexClient");
        }


        public Chapter Chapter { get; set; } = new Chapter();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<string> ChapterImages { get; set; } = new();
        public string? PreviousChapterId { get; set; }
        public string? NextChapterId { get; set; }

        public async Task<IActionResult> OnGet(string chapterId, int count = 1, int? showRepliesFor = null)
        {
            if (string.IsNullOrEmpty(chapterId))
            {
                return NotFound();
            }

            Chapter = _context.Chapters.Include(c => c.Comments).FirstOrDefault(c => c.MangaDexChapterId == chapterId);
            if (Chapter == null)
                return RedirectToPage("/error");

            Comments = _context.Comments
            .Include(c => c.Chapter)
            .Include(c => c.User)
            .Include(c => c.Likes)

            .Where(c => c.ChapterId == Chapter.Id && c.ParentCommentId == null)

            .Include(c => c.Replies)
                .ThenInclude(r => r.Chapter) 

            .Include(c => c.Replies)
                .ThenInclude(r => r.User) 
            .Include(c => c.Replies)
                .ThenInclude(r => r.Likes)
                    .ThenInclude(l => l.User)

            .ToList();

            //paging chapter
            var chapterList = _context.Chapters.Where(c => c.MangaId == Chapter.MangaId).OrderBy(c => c.ChapterNumber).ToList();

            var currentIndex = chapterList.FindIndex(c => c.MangaDexChapterId == chapterId);

            PreviousChapterId = currentIndex > 0
                ? chapterList[currentIndex - 1].MangaDexChapterId
                : null;

            NextChapterId = currentIndex < chapterList.Count - 1
                ? chapterList[currentIndex + 1].MangaDexChapterId
                : null;

            // call api mangadex chapter
            //try
            //{
            //    if (!string.IsNullOrEmpty(Chapter.MangaDexChapterId))
            //    {

            //        var serverResponse = await _httpClient.GetFromJsonAsync<MangaDexServerResponse>(
            //            $"at-home/server/{chapterId}");
            //        if (serverResponse?.Chapter?.Data != null)
            //        {
            //            var baseUrl = serverResponse.BaseUrl;
            //            var hash = serverResponse.Chapter.Hash;

            //            ChapterImages = serverResponse.Chapter.Data
            //                .Select(fileName => $"{baseUrl}/data/{hash}/{fileName}")
            //                .ToList();
            //        }
            //        else
            //            return RedirectToPage("/Public/Error");

            //    }
            //}
            //catch (HttpRequestException ex)
            //{
            //    return RedirectToPage("/Public/Error");
            //}



            return Page();
        }

        public async Task<IActionResult> OnPostCommentAsync([FromBody] ReplyRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            var comment = new Comment
            {
                UserId = userId,
                ChapterId = request.ChapterId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            Console.WriteLine($"Adding comment: {request.Content} for user {userId}");
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                id = comment.Id,
                content = comment.Content,
                userName = user?.UserName ?? "Anonymous",
                createdAt = comment.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            });
        }


        public async Task<IActionResult> OnPostReply([FromBody] ReplyRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            var parentComment = await _context.Comments.FindAsync(request.CommentId);
            if (parentComment == null) return NotFound();

            var reply = new Comment
            {
                UserId = userId,
                Content = request.Content,
                ChapterId = request.ChapterId,
                ParentCommentId = parentComment.Id,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new JsonResult(new
            {
                id = reply.Id,
                content = reply.Content,
                userName = user?.UserName ?? "Anonymous",
                chapterId = request?.ChapterId,
                createdAt = reply.CreatedAt.ToLocalTime()
            });
        }

        public class ReplyRequest
        {
            public int CommentId { get; set; }
            public string Content { get; set; } = string.Empty;

            public int ChapterId { get; set; }
            public string MangaDexChapterId { get; set; }


        }


        public async Task<IActionResult> OnPostDeleteCommentAsync()
        {
            var comment = await _context.Comments.FindAsync(Input.Id);
            if (comment == null)
                return new JsonResult(new { success = false, message = "Comment not found." });

            // Lấy tất cả comment con trực tiếp
            var replies = await _context.Comments
                .Where(r => r.ParentCommentId == comment.Id)
                .ToListAsync();

            // Xóa tất cả reply
            _context.Comments.RemoveRange(replies);

            // Xóa comment gốc
            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }



        public async Task<IActionResult> OnPostEditCommentAsync()
        {
            if (!ModelState.IsValid)
            {
                // TẠM THỜI GỠ BỎ để debug và xem log lỗi
                // return new JsonResult(new { success = false, message = "Invalid data." });

                // Lấy danh sách các lỗi để biết trường nào bị thiếu
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });

                // Gửi lỗi chi tiết về client (Console) để biết vấn đề là gì
                return new JsonResult(new { success = false, message = "Invalid data.", errors = errors });
            }

            var comment = await _context.Comments.FindAsync(Input.Id);
            if (comment == null)
                return new JsonResult(new { success = false, message = "Comment not found." });

            comment.Content = Input.Content;
            _context.Update(comment);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                content = comment.Content,
                commentId = comment.Id,
                mangadexChapterId = Input.MangaDexChapterId
            });
        }


        public async Task<IActionResult> OnPostLikeComment([FromBody] LikeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToPage("/Auth/Login");

            var comment = await _context.Comments.FindAsync(request.CommentId);
            if (comment == null)
                return NotFound();

            var existing = await _context.CommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == request.CommentId && l.UserId == userId);

            if (existing != null)
            {
                // Nếu người dùng bấm cùng loại reaction => bỏ like/dislike
                if (existing.ReactionType == request.ReactionType)
                {
                    _context.CommentLikes.Remove(existing);
                }
                else
                {
                    // Đổi từ like → dislike hoặc ngược lại
                    existing.ReactionType = request.ReactionType;
                }
            }
            else
            {
                // Chưa từng like/dislike => thêm mới
                _context.CommentLikes.Add(new CommentLike
                {
                    CommentId = request.CommentId,
                    UserId = userId,
                    ReactionType = request.ReactionType
                });
            }

            await _context.SaveChangesAsync();

            // Đếm lại số like/dislike
            var likeCount = await _context.CommentLikes
                .CountAsync(l => l.CommentId == request.CommentId && l.ReactionType == 1);
            var dislikeCount = await _context.CommentLikes
                .CountAsync(l => l.CommentId == request.CommentId && l.ReactionType == -1);

            var current = await _context.CommentLikes
       .Where(l => l.CommentId == request.CommentId && l.UserId == userId)
       .Select(l => l.ReactionType)
       .FirstOrDefaultAsync();

            return new JsonResult(new
            {
                likeCount,
                dislikeCount,
                reactionType = current
            });
        }


        public class LikeRequest
        {
            public int CommentId { get; set; }
            public int ReactionType { get; set; }
        }



    }


    public class MangaDexServerResponse
    {
        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; }

        [JsonPropertyName("chapter")]
        public MangaDexServerChapter Chapter { get; set; }
    }

    public class MangaDexServerChapter
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("data")]
        public List<string> Data { get; set; }

        [JsonPropertyName("dataSaver")]
        public List<string> DataSaver { get; set; }
    }

}
