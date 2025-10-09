using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.ViewModels.Auth;
using PRN_MANGA_PROJECT.Services.Auth;

namespace PRN_MANGA_PROJECT.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        public ForgotPasswordDTO Input { get; set; } = new ForgotPasswordDTO();
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(IUserService userService, IEmailSender emailSender)
        {
            _userService = userService;
            _emailSender = emailSender;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            var user = await _userService.FindByEmail(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError("Input.Email", "Your email is not existed");
                return Page();

            }
            await _userService.UpdateToken(user);

            var newToken = await _userService.GenerateResetPasswordToken(user);

            var encodedToken = System.Web.HttpUtility.UrlEncode(newToken);

            if (newToken == null)
            {
                return RedirectToPage("Public/HomePage");
            }
            var resetLink = Url.Page("/Auth/ResetPassword", pageHandler: null
                , values: new { email = Input.Email, token = encodedToken }
                , protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(Input.Email, "Reset Your Password",
     $"Click this link to reset your password: <a href='{resetLink}'>Reset Password</a>");


            ModelState.AddModelError("Input.Email", "Request is sended in your email");

            return Page();
        }
    }
}
