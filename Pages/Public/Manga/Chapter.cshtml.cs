using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using System.Security.Claims;
using System.Security.Claims;
using System.Xml.Linq;


namespace PRN_MANGA_PROJECT.Pages.Public.Manga
{
    public class ChapterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]

        public CommentViewModel Input { get; set; } = new CommentViewModel();

        private readonly IHubContext<CommentHub> _hubContext;

        public ChapterModel(ApplicationDbContext context , IHubContext<CommentHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public Models.Entities.Chapter Chapter { get; set; } = new Models.Entities.Chapter();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Comment> ChildComments { get; set; } = new List<Comment>();
        public List<ChapterImage> ChapterImages { get; set; } = new List<ChapterImage>();
        public int? PreviousChapterId { get; set; }
        public int? NextChapterId { get; set; }
        public int MangaId { get; set; }
        public List<Models.Entities.Chapter> AllChapters { get; set; } = new List<Models.Entities.Chapter>();
        
        public async Task<IActionResult> OnGet(int chapterId)
        {
            if (chapterId == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            Chapter = _context.Chapters
                .Include(c => c.Comments)
                .Include(c => c.ChapterImages)
                .FirstOrDefault(c => c.Id == chapterId);
                
            if (Chapter == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            
            ChapterImages = Chapter.ChapterImages?.OrderBy(ci => ci.PageNumber).ToList() ?? new List<ChapterImage>();
            
            // Get manga ID for navigation
            MangaId = Chapter.MangaId;
            
            // Get all chapters for this manga
            AllChapters = _context.Chapters
                .Where(c => c.MangaId == MangaId && c.IsActive)
                .OrderBy(c => c.Id)
                .ToList();
            
            // Get previous and next chapter IDs
            var allChapterIds = AllChapters.Select(c => c.Id).ToList();
            var currentIndex = allChapterIds.IndexOf(chapterId);
            if (currentIndex > 0)
            {
                PreviousChapterId = allChapterIds[currentIndex - 1];
            }
            if (currentIndex < allChapterIds.Count - 1)
            {
                NextChapterId = allChapterIds[currentIndex + 1];
            }

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
            .OrderByDescending(c => c.CreatedAt)
           .ToList();
            ViewData["ChapterId"] = chapterId;
            ViewData["CurrentUserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Page();
        }


        public IActionResult OnGetComments(int chapterId, string userId)
        {
            var comments = _context.Comments
                .Where(c => c.ChapterId == chapterId && c.ParentCommentId == null)
                .Include(c => c.User)
                           .Include(c => c.Likes)
                .Include(c => c.Replies)
                        .ThenInclude(r => r.Likes)
                    .ThenInclude(r => r.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            var viewData = new ViewDataDictionary<IEnumerable<PRN_MANGA_PROJECT.Models.Entities.Comment>>(
       metadataProvider: new EmptyModelMetadataProvider(),
       modelState: new ModelStateDictionary())
            {
                Model = comments
            };

            // Truyền thêm dữ liệu phụ
            viewData["ChapterId"] = chapterId;
            viewData["CurrentUserId"] = userId;

            return new PartialViewResult
            {
                ViewName = "Shared/Partial/_CommentPartial",
                ViewData = viewData,
                TempData = TempData
            };
        }



        public async Task<IActionResult> OnPostComment()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           if(userId == null)
            {
                return RedirectToPage("/Auth/Login");

            }
            var comment = new Comment
            {
                UserId = userId,
                ChapterId = Input.ChapterId,
                Content = Input.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Comments.Add(comment);
            _context.SaveChanges();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostReply()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                return RedirectToPage("/Auth/Login");

            }
            var comment = new Comment
            {
                UserId = userId,
                ChapterId = Input.ChapterId,
                Content = Input.Content,
                ParentCommentId = Input.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Comments.Add(comment);
            _context.SaveChanges();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostDeleteComment()
        {
            var comment = await _context.Comments.FindAsync(Input.Id);
            if (comment == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            var replies = await _context.Comments
       .Where(r => r.ParentCommentId == comment.Id)
       .ToListAsync();

            foreach (var reply in replies)
            {
                reply.ParentCommentId = null;
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostEditComment(int commentId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var comment = await _context.Comments.FindAsync(Input.Id);
            if (comment == null)
                return RedirectToPage("/Auth/Login");

            comment.Content = Input.Content;
            _context.Update(comment);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostLikeComment(LikeRequest request)
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
                if (existing.ReactionType == request.ReactionType)
                {
                    _context.CommentLikes.Remove(existing);
                }
                else
                {
                    existing.ReactionType = request.ReactionType;
                }
            }
            else
            {
                _context.CommentLikes.Add(new CommentLike
                {
                    CommentId = request.CommentId,
                    UserId = userId,
                    ReactionType = request.ReactionType
                });
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"SignalR send LoadComments for CommentId={request.CommentId}");

            await _hubContext.Clients.All.SendAsync("LoadComments");
            return new JsonResult(new { success = true });

        }


        public class LikeRequest
        {
            public int CommentId { get; set; }
            public int ReactionType { get; set; }
        }

    }
}
