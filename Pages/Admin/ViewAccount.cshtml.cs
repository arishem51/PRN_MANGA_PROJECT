using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using PRN_MANGA_PROJECT.Services.CRUD;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages.Admin
{
    public class ViewAccountModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ViewAccountModel(IAccountService accountService, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _accountService = accountService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<AccountViewModel> Accounts { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1; // ✅ Trang hiện tại

        public int PageSize { get; set; } = 5;   // ✅ Số bản ghi mỗi trang
        public int TotalPages { get; set; }       // ✅ Tổng số trang

        public List<string> AllRoles { get; set; } = new();

        public async Task OnGetAsync() => await LoadDataAsync();

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            await _accountService.Update(user);
            return RedirectToPage(); // Reload page
        }

        public async Task<IActionResult> OnPostUnlockAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            await _accountService.Update(user);
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            // ✅ Lọc theo SearchTerm
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                users = users
                    .Where(u => u.UserName != null && u.UserName.ToLower().Contains(SearchTerm.ToLower()))
                    .ToList();
            }

            // ✅ Chuyển đổi sang ViewModel
            var tempList = new List<AccountViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.Any() ? string.Join(", ", roles) : "(Chưa gán)";

                // ✅ Lọc theo vai trò
                if (!string.IsNullOrEmpty(RoleFilter) && !roles.Contains(RoleFilter))
                    continue;

                tempList.Add(new AccountViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = roleName,
                    IsActive = user.IsActive
                });
            }

            // ✅ Phân trang
            int totalRecords = tempList.Count;
            TotalPages = (int)System.Math.Ceiling(totalRecords / (double)PageSize);

            Accounts = tempList
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
