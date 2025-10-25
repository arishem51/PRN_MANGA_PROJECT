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
        public async Task<IActionResult> OnGet(int chapterId)
        {
            if (chapterId == null)
            {
                return NotFound();
            }

            Chapter = _context.Chapters.Include(c => c.Comments).FirstOrDefault(c => c.Id == chapterId);
            Comments = _context.Comments
            .Where(c => c.ChapterId == chapterId && c.ParentCommentId == null).Include(c => c.Replies).ToList();
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
