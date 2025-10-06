using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.ViewModels.Auth
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; }
    }
}
