using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using PRN_MANGA_PROJECT.Models.ViewModels.Auth;
using PRN_MANGA_PROJECT.Services.Auth;

namespace PRN_MANGA_PROJECT.Pages.Auth
{
    public class ResetPasswordModel : PageModel
    {
        [BindProperty]
        public ResetPasswordDTO Input { get; set; } = new ResetPasswordDTO();
        private readonly IUserService _userService;

        public ResetPasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(string email , string token)
        {
            Input.Token = token;
            Input.Email = email;    

            if(token.IsNullOrEmpty() || email.IsNullOrEmpty())
            {
                return RedirectToPage("/Public/Error");
            }
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()  
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return Page();
            }

            var user = await _userService.FindByEmail(Input.Email);
            if (user == null) {
                ModelState.AddModelError("Input.Email", "Your Email not existed");
                return Page();
            }

            var response = await _userService.ResetPassword(user , Input.Token , Input.NewPassword);
            if (response.Succeeded)
            {
                await _userService.UpdateToken(user);
                return RedirectToPage("/Auth/Login");
            }
            if (!response.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Reset Password Failed!");
                return Page();
            }
            return Page();

        }

    }
}
