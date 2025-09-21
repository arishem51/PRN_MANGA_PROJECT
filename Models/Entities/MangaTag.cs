namespace PRN_MANGA_PROJECT.Models.Entities
{
    public class MangaTag
    {
        public int MangaId { get; set; }
        public int TagId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Manga Manga { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
