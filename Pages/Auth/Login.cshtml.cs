using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels.Auth;
using PRN_MANGA_PROJECT.Services.Auth;
using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace PRN_MANGA_PROJECT.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly SignInManager<User> _signInManager;


        [BindProperty]
        public LoginDTO Input { get; set; } = new LoginDTO();

        public LoginModel(IUserService userService , SignInManager<User> signInManager)
        {
            _userService = userService;
            _signInManager = signInManager;

        }


        public void OnGet()
        {
            // save binding loginDTO when remember Me 
            if (Request.Cookies.ContainsKey("Username"))
            {
                Input.Username = Request.Cookies["Username"];
                Input.RememberMe = true; 
            }
        }

        public async Task<IActionResult> OnPost()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }

            var rememberMe = Input.RememberMe;
            if (rememberMe)
            {
                Response.Cookies.Append("Username", Input.Username, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(14)
                });
            }
            else
            {
                Response.Cookies.Delete("RememberedUsername");
            }

            // check signin 
            var check = await _userService.Login(Input.Username, Input.Password , rememberMe);

            // check confirm email
            bool checkEmail = await _userService.CheckEmailConfirmation(Input.Username);
            if (check && checkEmail)
            {
                return RedirectToPage("/Index");
            }

            if (!check)
            {
                ModelState.AddModelError(string.Empty, "Username or password is incorrect.");

            }
            if (!checkEmail)
            {
                ModelState.AddModelError(string.Empty, "Please confirm email to sign in");

            }
            return Page();
        }

        public IActionResult OnPostExternalLogin(string provider)
        {
            //create URl callback to redirect after user login successful
            var redirectUrl = Url.Page("./LoginByGoogle", "Callback", new { rememberMe = Input.RememberMe });
            //create properties include security info (token , state) and url after login by gg
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties); // send request to middleware followed by provider
                                                              // , if provider is GG => ggHandler 
        }





    }
}
