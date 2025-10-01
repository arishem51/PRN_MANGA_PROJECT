using Microsoft.AspNetCore.Identity;

namespace PRN_MANGA_PROJECT.Models.Entities
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public bool Gender { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
