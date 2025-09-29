using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.ViewModels.Auth
{
    public class RegisterDTO
    {
        [Required(ErrorMessage ="First Name is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }


        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password" , ErrorMessage = "Confirm Password is not matching")]
        public string? ConfirmPassword { get; set; }

        public bool Gender { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
