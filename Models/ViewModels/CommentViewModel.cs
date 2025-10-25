namespace PRN_MANGA_PROJECT.Models.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int ChapterId { get; set; }
        public string MangaDexChapterId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
    }
}
