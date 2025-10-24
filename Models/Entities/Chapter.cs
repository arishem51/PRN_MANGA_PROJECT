using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.Entities
{
    public class Chapter
    {
        public int Id { get; set; }
        
        [Required]
        public int MangaId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? ChapterNumber { get; set; }
        [Required]
        [StringLength(100)]
        public string MangaDexChapterId { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Content { get; set; } // For text-based chapters
        
        public int? PageCount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Manga Manga { get; set; } = null!;
        public virtual ICollection<ChapterImage> ChapterImages { get; set; } = new List<ChapterImage>();
        public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
