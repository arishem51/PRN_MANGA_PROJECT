using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using PRN_MANGA_PROJECT.Services.EmailService;
using PRN_MANGA_PROJECT.Services.CRUD;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PRN_MANGA_PROJECT.Pages.Admin
{
    public class CreateAccountModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService; // ✅ Thêm Service để kích hoạt SignalR

        public CreateAccountModel(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            IAccountService accountService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _accountService = accountService;
        }

        [BindProperty]
        public AccountViewModel Input { get; set; } = new AccountViewModel();

        public List<string> AllRoles { get; set; } = new List<string>();

        public void OnGet()
        {
            AllRoles = _roleManager.Roles
                .Select(r => r.Name!)
                .Where(r => r != "admin")
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ✅ Load lại danh sách role khi submit
            AllRoles = _roleManager.Roles
                .Select(r => r.Name!)
                .Where(r => r != "admin")
                .ToList();

            if (!ModelState.IsValid)
                return Page();

            

            // ✅ Sinh mật khẩu ngẫu nhiên
            string password = GenerateRandomPassword();

            // ✅ Gọi AccountService để tạo user (service này sẽ kích hoạt SignalR)
            var result = await _accountService.Create(Input.Username, Input.Email, password, Input.Role);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return Page();
            }

            // ✅ Gửi email thông báo
            string subject = "Tài khoản mới của bạn";
            string body = $"""
            Xin chào {Input.Username},

            Tài khoản của bạn đã được tạo thành công.

            🔹 Tên đăng nhập: {Input.Username}
            🔹 Mật khẩu: {password}
            🔹 Vai trò: {Input.Role}

            Vui lòng đăng nhập và đổi mật khẩu ngay sau khi đăng nhập.
            """;

            await _emailService.SendEmailAsync(Input.Email, subject, body);

            TempData["SuccessMessage"] = "✅ Tạo tài khoản thành công!";
            return RedirectToPage("/ViewAccount");
        }

        // ✅ Hàm sinh mật khẩu hợp lệ cho ASP.NET Identity
        private string GenerateRandomPassword(int length = 10)
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string all = lower + upper + digits;

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            var password = new string(bytes.Select(b => all[b % all.Length]).ToArray());
            return "Aa1" + password.Substring(3); // đảm bảo có chữ hoa + thường + số
        }
    }
}
