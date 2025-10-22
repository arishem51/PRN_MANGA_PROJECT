namespace PRN_MANGA_PROJECT.Models.ViewModels
{
    public class ChapterViewModel
    {
        public int Id { get; set; }
        public int MangaId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ChapterNumber { get; set; }
        public string? Content { get; set; }
        public int? PageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ImageCount { get; set; }
        public List<ChapterImageViewModel> ChapterImages { get; set; } = new List<ChapterImageViewModel>();
        public string MangaTitle { get; set; } = string.Empty;
    }

    public class CreateChapterViewModel
    {
        public int MangaId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ChapterNumber { get; set; }
        public string? Content { get; set; }
        public int? PageCount { get; set; }
    }

    public class UpdateChapterViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? ChapterNumber { get; set; }
        public string? Content { get; set; }
        public int? PageCount { get; set; }
    }
}
