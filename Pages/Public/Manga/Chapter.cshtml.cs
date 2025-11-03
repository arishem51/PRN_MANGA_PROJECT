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
using PRN_MANGA_PROJECT.Services.CommentService;
using PRN_MANGA_PROJECT.Services.HistoryService;
using System.Security.Claims;
using System.Security.Claims;
using System.Xml.Linq;


namespace PRN_MANGA_PROJECT.Pages.Public.Manga
{
    public class ChapterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommentService _commentService;
        private readonly IReadingHistoryService _readingHistoryService;

        [BindProperty]

        public CommentViewModel Input { get; set; } = new CommentViewModel();

        private readonly IHubContext<CommentHub> _hubContext;

        public ChapterModel(ApplicationDbContext context,
                    IHubContext<CommentHub> hubContext,
                    ICommentService commentService,
                    IReadingHistoryService readingHistoryService)
        {
            _context = context;
            _commentService = commentService;
            _hubContext = hubContext;
            _readingHistoryService = readingHistoryService;
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


            //add history
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await _readingHistoryService.AddOrUpdateHistoryAsync(userId, MangaId, chapterId);
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
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            await _commentService.AddCommentAsync(userId, Input.ChapterId, Input.Content);
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostReply()
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            await _commentService.ReplyAsync(userId, Input.ChapterId, Input.Content, Input.ParentCommentId!.Value);
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostDeleteComment()
        {
            await _commentService.DeleteCommentAsync(Input.Id);
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostEditComment()
        {
            if (!ModelState.IsValid) return Page();
            await _commentService.EditCommentAsync(Input.Id, Input.Content);
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostLikeComment(LikeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            await _commentService.LikeCommentAsync(userId, request.CommentId, request.ReactionType);
            return new JsonResult(new { success = true });
        }


        public class LikeRequest
        {
            public int CommentId { get; set; }
            public int ReactionType { get; set; }
        }

    }
}
