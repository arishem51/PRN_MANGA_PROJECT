using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace PRN_MANGA_PROJECT.Pages
{
    public class EditAccountPageModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditAccountPageModel(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public EditAccountModel Input { get; set; } = new EditAccountModel();

        public List<string> AllRoles { get; set; } = new List<string>();

        // ✅ Load thông tin user + lọc bỏ role "admin"
        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // ✅ Chỉ lấy các role được phép hiển thị
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

            var updateResult = await _userManager.UpdateAsync(user);
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

            // ✅ Cập nhật role
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Ngăn admin role bị thêm vào bằng cách kiểm tra trước
            if (Input.Role != "admin")
            {
                if (!await _roleManager.RoleExistsAsync(Input.Role))
                    await _roleManager.CreateAsync(new IdentityRole(Input.Role));

                await _userManager.AddToRoleAsync(user, Input.Role);
            }

            return RedirectToPage("/ViewAccount");
        }
    }
}
