namespace PRN_MANGA_PROJECT.Models.ViewModels
{
    public class ChapterImageViewModel
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int Order { get; set; }
    }
}
