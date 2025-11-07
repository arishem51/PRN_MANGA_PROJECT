using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using PRN_MANGA_PROJECT.Services.CRUD;

namespace PRN_MANGA_PROJECT.Pages.Admin
{
    [Authorize(Roles = "Admin")]
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
            AllRoles = await _roleManager.Roles
                .Where(r => r.Name != "admin") // Không cho chọn admin trong dropdown
                .Select(r => r.Name!)
                .ToListAsync();

            // ✅ Lấy danh sách user qua service (đã có filter sẵn nếu bạn thêm vào service)
            var allAccounts = await _accountService.GetAllWithRolesAsync();
            var keyword = SearchTerm?.Trim().ToLower();
            // ✅ Áp dụng tìm kiếm và lọc vai trò
            if (!string.IsNullOrWhiteSpace(keyword))
                allAccounts = allAccounts
                    .Where(u => !string.IsNullOrEmpty(u.Username) &&
                                u.Username.ToLower().Contains(keyword));


            if (!string.IsNullOrEmpty(RoleFilter))
                allAccounts = allAccounts
                    .Where(u => u.Role.Equals(RoleFilter, StringComparison.OrdinalIgnoreCase));

            // ✅ Phân trang
            int totalRecords = allAccounts.Count();
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            Accounts = allAccounts
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
