using Microsoft.AspNetCore.SignalR;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories.CommentRepository;

namespace PRN_MANGA_PROJECT.Services.CommentService
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repo;
        private readonly IHubContext<CommentHub> _hubContext;

        public CommentService(ICommentRepository repo, IHubContext<CommentHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        public async Task<bool> AddCommentAsync(string userId, int chapterId, string content)
        {
            var comment = new Comment
            {
                UserId = userId,
                ChapterId = chapterId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repo.AddAsync(comment);
            await _repo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return true;
        }

        public async Task<bool> ReplyAsync(string userId, int chapterId, string content, int parentId)
        {
            var comment = new Comment
            {
                UserId = userId,
                ChapterId = chapterId,
                Content = content,
                ParentCommentId = parentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repo.AddAsync(comment);
            await _repo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _repo.FindAsync(id);
            if (comment == null) return false;

            var replies = await _repo.GetRepliesAsync(comment.Id);
            foreach (var reply in replies)
            {
                reply.ParentCommentId = null;
            }

            var context = (ApplicationDbContext)typeof(CommentRepository)
                .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(_repo)!;

            context.Comments.Remove(comment);
            await _repo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return true;
        }

        public async Task<bool> EditCommentAsync(int id, string newContent)
        {
            var comment = await _repo.FindAsync(id);
            if (comment == null) return false;

            comment.Content = newContent;
            await _repo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return true;
        }

        public async Task<bool> LikeCommentAsync(string userId, int commentId, int reactionType)
        {
            var comment = await _repo.FindAsync(commentId);
            if (comment == null) return false;

            var existing = await _repo.GetUserReactionAsync(commentId, userId);

            if (existing != null)
            {
                if (existing.ReactionType == reactionType)
                    await _repo.RemoveReactionAsync(existing);
                else
                    existing.ReactionType = reactionType;
            }
            else
            {
                await _repo.AddReactionAsync(new CommentLike
                {
                    CommentId = commentId,
                    UserId = userId,
                    ReactionType = reactionType
                });
            }

            await _repo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("LoadComments");
            return true;
        }
    }
}
