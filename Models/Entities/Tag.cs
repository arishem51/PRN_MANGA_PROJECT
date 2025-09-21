using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(20)]
        public string? Color { get; set; } // Hex color for UI display
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<MangaTag> MangaTags { get; set; } = new List<MangaTag>();
    }
}
