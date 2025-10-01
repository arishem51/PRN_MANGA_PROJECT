using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using PRN_MANGA_PROJECT.Services.EmailService;

namespace PRN_MANGA_PROJECT.Pages
{
    public class CreateAccountModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public CreateAccountModel(UserManager<User> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required]
            [Display(Name = "Tên đăng nhập")]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            string password = GenerateRandomPassword();

            var user = new User
            {
                UserName = Input.Username,
                Email = Input.Email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                string subject = "Tài khoản mới của bạn";
                string body = $"Xin chào,\n\nTài khoản của bạn đã được tạo.\nTên đăng nhập: {Input.Username}\nMật khẩu: {password}\n\nVui lòng đăng nhập và đổi mật khẩu ngay.";
                await _emailService.SendEmailAsync(Input.Email, subject, body);

                return RedirectToPage("/ViewAccount");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
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
