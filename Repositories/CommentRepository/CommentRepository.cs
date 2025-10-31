using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories.CommentRepository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public async Task<Comment?> FindAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<List<Comment>> GetRepliesAsync(int parentId)
        {
            return await _context.Comments
                .Where(r => r.ParentCommentId == parentId).ToListAsync();
        }

        public async Task<CommentLike?> GetUserReactionAsync(int commentId, string userId)
        {
            return await _context.CommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);
        }

        public async Task AddReactionAsync(CommentLike like)
        {
            await _context.CommentLikes.AddAsync(like);
        }

        public async Task RemoveReactionAsync(CommentLike like)
        {
            _context.CommentLikes.Remove(like);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
