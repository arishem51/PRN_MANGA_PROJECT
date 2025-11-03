using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories.CommentRepository
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment);
        Task<Comment?> FindAsync(int id);
        Task<List<Comment>> GetRepliesAsync(int parentId);
        Task<CommentLike?> GetUserReactionAsync(int commentId, string userId);
        Task AddReactionAsync(CommentLike like);
        Task RemoveReactionAsync(CommentLike like);
        Task SaveChangesAsync();
    }
}
