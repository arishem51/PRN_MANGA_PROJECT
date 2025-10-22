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

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [UniqueEmail(ErrorMessage = "Email này đã được sử dụng, vui lòng chọn email khác")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
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
                return new ValidationResult(ErrorMessage ?? "Email đã tồn tại trong hệ thống");
            }

            return ValidationResult.Success;
        }
    }
}
