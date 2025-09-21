using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.Entities
{
    public class Bookmark
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int MangaId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastReadAt { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Manga Manga { get; set; } = null!;
    }
}
