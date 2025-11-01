namespace PRN_MANGA_PROJECT.Services.CommentService
{
    public interface ICommentService
    {
        Task<bool> AddCommentAsync(string userId, int chapterId, string content);
        Task<bool> ReplyAsync(string userId, int chapterId, string content, int parentId);
        Task<bool> DeleteCommentAsync(int id);
        Task<bool> EditCommentAsync(int id, string newContent);
        Task<bool> LikeCommentAsync(string userId, int commentId, int reactionType);
    }
}
