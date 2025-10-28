using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN_MANGA_PROJECT.Models.Entities
{
    public class CommentLike
    {
        public int Id { get; set; }

        [Required]
        public int CommentId { get; set; }

        [Required]
        public string UserId { get; set; }

        public int ReactionType { get; set; } = 1;

        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
