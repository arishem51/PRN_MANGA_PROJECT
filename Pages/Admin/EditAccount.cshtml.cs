using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using PRN_MANGA_PROJECT.Services.CRUD;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace PRN_MANGA_PROJECT.Pages.Admin
{
    public class EditAccountPageModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAccountService _accountService; // ✅ Thêm service để phát SignalR

        public EditAccountPageModel(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IAccountService accountService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _accountService = accountService;
        }

        [BindProperty]
        public EditAccountModel Input { get; set; } = new EditAccountModel();

        public List<string> AllRoles { get; set; } = new List<string>();

        // ✅ Load thông tin user + lọc bỏ role "admin"
        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            AllRoles = _roleManager.Roles
                .Select(r => r.Name!)
                .Where(r => r != "admin")
                .ToList();

            var userRoles = await _userManager.GetRolesAsync(user);

            Input = new EditAccountModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Role = userRoles.FirstOrDefault() ?? ""
            };

            return Page();
        }

        // ✅ Cập nhật user + role
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AllRoles = _roleManager.Roles
                    .Select(r => r.Name!)
                    .Where(r => r != "admin")
                    .ToList();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(Input.Id);
            if (user == null) return NotFound();

            user.UserName = Input.Username;
            user.Email = Input.Email;

            // ✅ Gọi qua AccountService để update (service này gọi repo → phát SignalR)
            var updateResult = await _accountService.Update(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);

                AllRoles = _roleManager.Roles
                    .Select(r => r.Name!)
                    .Where(r => r != "admin")
                    .ToList();
                return Page();
            }

            // ✅ Cập nhật role qua service (có phát SignalR)
            if (Input.Role != "admin")
            {
                await _accountService.AssignRoleAsync(user, Input.Role);
            }

            TempData["SuccessMessage"] = "✅ Cập nhật tài khoản thành công!";
            return RedirectToPage("/ViewAccount");
        }
    }
}
