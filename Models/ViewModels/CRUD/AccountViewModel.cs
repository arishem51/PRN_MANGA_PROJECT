using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PRN_MANGA_PROJECT.Models.Entities;
using System.Linq;

namespace PRN_MANGA_PROJECT.Models.ViewModels.CRUD
{
    public class AccountViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "UserName can't empty")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [UniqueUsername(ErrorMessage = "UserName is existed")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email can't empty")]
        [EmailAddress(ErrorMessage = "Email format wrong")]
        [UniqueEmail(ErrorMessage = "Email is existed")]
        public string Email { get; set; }

        [Required(ErrorMessage = "please choose role")]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ✅ Custom ValidationAttribute kiểm tra email trùng trong hệ thống.
    /// </summary>
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var email = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(email))
                return ValidationResult.Success;

            // ✅ Lấy UserManager từ DI container
            var userManager = validationContext.GetService(typeof(UserManager<User>)) as UserManager<User>;
            if (userManager == null)
                return ValidationResult.Success;

            // ✅ Kiểm tra trùng email
            var existingUser = userManager.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                return new ValidationResult(ErrorMessage ?? "Email is existed in system");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// ✅ Custom ValidationAttribute kiểm tra username trùng trong hệ thống.
    /// </summary>
    public class UniqueUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var username = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(username))
                return ValidationResult.Success;

            // ✅ Lấy UserManager từ DI container
            var userManager = validationContext.GetService(typeof(UserManager<User>)) as UserManager<User>;
            if (userManager == null)
                return ValidationResult.Success;

            // ✅ Kiểm tra trùng username
            var existingUser = userManager.Users.FirstOrDefault(u => u.UserName == username);
            if (existingUser != null)
            {
                return new ValidationResult(ErrorMessage ?? "UserName is existed in system");
            }

            return ValidationResult.Success;
        }
    }
}
