using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Services.EmailService;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages
{
    public class CreateAccountModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public CreateAccountModel(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn vai trò")]
            public string RoleName { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // ✅ Tự sinh mật khẩu ngẫu nhiên
            string password = GenerateRandomPassword();

            // ✅ Tạo user mới
            var user = new User
            {
                UserName = Input.Username,
                Email = Input.Email,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // ✅ Nếu role chưa có thì tạo mới
                if (!await _roleManager.RoleExistsAsync(Input.RoleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Input.RoleName));
                }

                // ✅ Gán role cho user
                await _userManager.AddToRoleAsync(user, Input.RoleName);

                // ✅ Gửi email thông báo tài khoản
                string subject = "Tài khoản mới của bạn";
                string body = $"""
                Xin chào {Input.Username},

                Tài khoản của bạn đã được tạo thành công.

                🔹 Tên đăng nhập: {Input.Username}
                🔹 Mật khẩu: {password}
                🔹 Vai trò: {Input.RoleName}

                Vui lòng đăng nhập và đổi mật khẩu ngay sau khi đăng nhập.
                """;

                await _emailService.SendEmailAsync(Input.Email, subject, body);

                TempData["SuccessMessage"] = "✅ Tạo tài khoản thành công!";
                return RedirectToPage("/ViewAccount");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private string GenerateRandomPassword(int length = 10)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return new string(bytes.Select(b => valid[b % valid.Length]).ToArray());
        }
    }
}
