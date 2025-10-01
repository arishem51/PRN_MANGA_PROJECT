using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Services.Auth;

namespace PRN_MANGA_PROJECT.Pages.Auth
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly IUserService _userService;

        public ConfirmEmailModel(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<IActionResult> OnGet(string userId , string token)
        {
            if(userId == null || token == null)
            {
                return RedirectToPage("/Public/");
            }

            var user = await _userService.FindUserById(userId);
            if(user == null)
            {
                return NotFound($"Không tìm thấy user với ID = {userId}");
            }
            var response = await _userService.ConfirmEmail(user, token);
            if(response.Succeeded)
            {
                return Page();
            }
            return RedirectToPage("/Public/Error");
        }
    }
}
