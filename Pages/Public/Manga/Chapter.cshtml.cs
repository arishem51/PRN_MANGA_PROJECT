using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;

namespace PRN_MANGA_PROJECT.Pages.Public.Manga
{
    public class ChapterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]

        public CommentViewModel Input { get; set; } = new CommentViewModel();

        public ChapterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Entities.Chapter Chapter { get; set; } = new Models.Entities.Chapter();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Comment> ChildComments { get; set; } = new List<Comment>();
        public List<ChapterImage> ChapterImages { get; set; } = new List<ChapterImage>();
        public int? PreviousChapterId { get; set; }
        public int? NextChapterId { get; set; }
        public int MangaId { get; set; }
        
        public async Task<IActionResult> OnGet(int chapterId)
        {
            if (chapterId == null)
            {
                return NotFound();
            }

            Chapter = _context.Chapters
                .Include(c => c.Comments)
                .Include(c => c.ChapterImages)
                .FirstOrDefault(c => c.Id == chapterId);
                
            if (Chapter == null)
            {
                return NotFound();
            }
            
            ChapterImages = Chapter.ChapterImages?.OrderBy(ci => ci.PageNumber).ToList() ?? new List<ChapterImage>();
            
            // Get manga ID for navigation
            MangaId = Chapter.MangaId;
            
            // Get previous and next chapter IDs
            var allChapters = _context.Chapters
                .Where(c => c.MangaId == MangaId && c.IsActive)
                .OrderBy(c => c.Id)
                .Select(c => c.Id)
                .ToList();
                
            var currentIndex = allChapters.IndexOf(chapterId);
            if (currentIndex > 0)
            {
                PreviousChapterId = allChapters[currentIndex - 1];
            }
            if (currentIndex < allChapters.Count - 1)
            {
                NextChapterId = allChapters[currentIndex + 1];
            }
            
            Comments = _context.Comments
                .Where(c => c.ChapterId == chapterId && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostComment()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = "62b6f877-65d4-4ead-a967-90e24c697b71";

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
            return RedirectToPage(new { chapterId = Input.ChapterId });

        }

        public async Task<IActionResult> OnPostReply()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var userId = "62b6f877-65d4-4ead-a967-90e24c697b71";

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
            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

        public async Task<IActionResult> OnPostDeleteComment()
        {
            var comment = await _context.Comments.FindAsync(Input.Id);
            if (comment == null)
            {
                return NotFound();
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
                return NotFound();

            comment.Content = Input.Content;
            _context.Update(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { chapterId = Input.ChapterId });
        }

    }
}
