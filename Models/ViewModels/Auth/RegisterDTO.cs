using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.ViewModels.Auth
{
    public class RegisterDTO
    {
        [Required(ErrorMessage ="First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        public string? LastName { get; set; }


        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&#]).+$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number and one special character")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password" , ErrorMessage = "Confirm Password is not matching")]
        public string? ConfirmPassword { get; set; }

        public bool Gender { get; set; } = true;


        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Phone number must start with 0 and contain 10-11 digits")] public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
