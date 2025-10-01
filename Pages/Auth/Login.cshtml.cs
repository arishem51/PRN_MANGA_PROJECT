using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels.Auth;
using PRN_MANGA_PROJECT.Services.Auth;

namespace PRN_MANGA_PROJECT.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public LoginDTO Input { get; set; } = new LoginDTO();

        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }
            var check = await _userService.Login(Input.Username, Input.Password);
            bool checkEmail = await _userService.checkEmailConfirmation(Input.Username);
            if (check && checkEmail)
            {
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, "Username or password are incorrect.");
            return Page();
        }
    }
}
