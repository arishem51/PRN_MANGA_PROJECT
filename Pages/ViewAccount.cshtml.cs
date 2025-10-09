using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using PRN_MANGA_PROJECT.Services.CRUD;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages
{
    [Authorize(Roles = "Admin")]
    public class ViewAccountModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<User> _userManager;

        public ViewAccountModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public List<AccountViewModel> Accounts { get; set; } = new();

        // ✅ Chỉ lấy các tài khoản đang hoạt động
        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var result = await _accountService.Delete(id); // gọi soft delete
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            await LoadDataAsync();
            return Page();
        }
        private async Task LoadDataAsync()
        {
            Accounts.Clear();

            var users = await _accountService.GetAll(); // đã lọc IsActive=true nếu bạn làm đúng service

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                Accounts.Add(new AccountViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = roles.Any() ? string.Join(", ", roles) : "(Chưa gán)"
                });
            }
        }
    }
}
