namespace PRN_MANGA_PROJECT.Models.ViewModels
{
    public class TagViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public int MangaCount { get; set; }
    }
}
