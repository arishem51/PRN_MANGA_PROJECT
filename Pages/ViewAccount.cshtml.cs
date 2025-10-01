using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages
{
    public class ViewAccountModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public ViewAccountModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public List<User> Users { get; set; } = new List<User>();

        public void OnGet()
        {
            Users = _userManager.Users.ToList();
        }

        // Xóa user trực tiếp
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Load lại danh sách sau khi xóa
            Users = _userManager.Users.ToList();
            return Page();
        }
    }
}
