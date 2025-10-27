using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.Entities
{
    public class Manga : IHasTimestamps
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        
        [StringLength(100)]
        public string? Author { get; set; }
        
        [StringLength(100)]
        public string? Artist { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Status { get; set; } // Ongoing, Completed, Hiatus, etc.
        
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public virtual ICollection<MangaTag> MangaTags { get; set; } = new List<MangaTag>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
    }
}
