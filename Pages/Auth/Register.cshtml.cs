using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.Auth;
using PRN_MANGA_PROJECT.Services.Auth;

namespace PRN_MANGA_PROJECT.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public RegisterDTO Input { get; set; } = new RegisterDTO();

        private readonly IUserService _userService;

        public RegisterModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid) { 
                return Page();
            }
            var newUser = new User
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                UserName = Input.Username , 
                Address = Input.Address ,
                Gender = true ,
                PhoneNumber = Input.PhoneNumber ,
            };
            
            var response = await _userService.Register(newUser, Input.Password);
            if (!response.Succeeded)
            {
                foreach (var error in response.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return Page(); 
            }

            return RedirectToPage("/Auth/Login");
        }
    }
}
