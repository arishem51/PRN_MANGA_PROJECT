using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using PRN_MANGA_PROJECT.Services.EmailService;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            // ✅ Load lại danh sách role
            AllRoles = _roleManager.Roles
                .Select(r => r.Name!)
                .Where(r => r != "admin")
                .ToList();

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState không hợp lệ:");
                foreach (var err in ModelState)
                {
                    foreach (var valErr in err.Value.Errors)
                        Console.WriteLine($" - {err.Key}: {valErr.ErrorMessage}");
                }
                return Page();
            }

            // ✅ Kiểm tra username/email trùng
            if (await _userManager.FindByNameAsync(Input.Username) != null)
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                Console.WriteLine($"❌ Username '{Input.Username}' đã tồn tại.");
                return Page();
            }

            if (await _userManager.FindByEmailAsync(Input.Email) != null)
            {
                ModelState.AddModelError("", "Email đã được sử dụng.");
                Console.WriteLine($"❌ Email '{Input.Email}' đã tồn tại.");
                return Page();
            }

            // ✅ Sinh mật khẩu ngẫu nhiên
            string password = GenerateRandomPassword();
            Console.WriteLine($"🔐 Mật khẩu sinh tự động: {password}");

            // ✅ Tạo user mới
            var user = new User
            {
                UserName = Input.Username,
                Email = Input.Email,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                Console.WriteLine("❌ Lỗi khi tạo tài khoản:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - {error.Code}: {error.Description}");
                    ModelState.AddModelError("", error.Description);
                }
                return Page();
            }

            // ✅ Tạo và gán role (nếu có)
            if (!string.IsNullOrEmpty(Input.Role) && Input.Role != "admin")
            {
                if (!await _roleManager.RoleExistsAsync(Input.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Input.Role));
                    Console.WriteLine($"✅ Role mới được tạo: {Input.Role}");
                }

                await _userManager.AddToRoleAsync(user, Input.Role);
                Console.WriteLine($"✅ User '{Input.Username}' được gán role '{Input.Role}'");
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
            Console.WriteLine($"📧 Đã gửi email thông báo đến {Input.Email}");

            TempData["SuccessMessage"] = "✅ Tạo tài khoản thành công!";
            return RedirectToPage("/ViewAccount");
        }

        // ✅ Sinh mật khẩu hợp lệ cho ASP.NET Identity
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
