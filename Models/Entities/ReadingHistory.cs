using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.Entities
{
    public class ReadingHistory
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int MangaId { get; set; }
        
        public int? ChapterId { get; set; }
        
        public int? PageNumber { get; set; }
        
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Manga Manga { get; set; } = null!;
        public virtual Chapter? Chapter { get; set; }
    }
}
