namespace PRN_MANGA_PROJECT.Models.ViewModels
{
    public class MangaViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MangaDexId { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Artist { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<TagViewModel> Tags { get; set; } = new List<TagViewModel>();
        public int ChapterCount { get; set; }
        public bool IsBookmarked { get; set; }
    }
}
