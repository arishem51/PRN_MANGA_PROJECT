using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Models.ViewModels.CRUD
{
    public class EditAccountModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "UserName can't empty")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [UniqueUsernameEdit(ErrorMessage = "UserName is existed")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email can't empty")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [UniqueEmailEdit(ErrorMessage = "Email is existed")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please choose roles")]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UniqueEmailEditAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (EditAccountModel)validationContext.ObjectInstance;
            if (value == null)
                return ValidationResult.Success;

            var email = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(email))
                return ValidationResult.Success;

            var userManager = validationContext.GetService(typeof(UserManager<User>)) as UserManager<User>;
            if (userManager == null)
                return ValidationResult.Success;

            // ✅ Kiểm tra trùng email nhưng bỏ qua user hiện tại
            var existingUser = userManager.Users.FirstOrDefault(u => u.Email == email && u.Id != model.Id);
            if (existingUser != null)
            {
                return new ValidationResult(ErrorMessage ?? "Email is existed");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// ✅ Check username trùng khi edit (loại trừ bản thân)
    /// </summary>
    public class UniqueUsernameEditAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (EditAccountModel)validationContext.ObjectInstance;
            if (value == null)
                return ValidationResult.Success;

            var username = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(username))
                return ValidationResult.Success;

            var userManager = validationContext.GetService(typeof(UserManager<User>)) as UserManager<User>;
            if (userManager == null)
                return ValidationResult.Success;

            // ✅ Kiểm tra trùng username nhưng bỏ qua user hiện tại
            var existingUser = userManager.Users.FirstOrDefault(u => u.UserName == username && u.Id != model.Id);
            if (existingUser != null)
            {
                return new ValidationResult(ErrorMessage ?? "UserName is existed");
            }

            return ValidationResult.Success;
        }
    }
    }
