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
                TempData["ErrorMessage"] = "Your email does not exist.";
                return RedirectToPage("/Auth/ForgotPassword"); 
            }
            await _userService.UpdateToken(user);

            var newToken = await _userService.GenerateResetPasswordToken(user);

            var encodedToken = System.Web.HttpUtility.UrlEncode(newToken);

            if (newToken == null)
            {
                TempData["ErrorMessage"] = "Unable to generate reset token.";
                return RedirectToPage("/Auth/ForgotPassword");
            }
            var resetLink = Url.Page("/Auth/ResetPassword", pageHandler: null
                , values: new { email = Input.Email, token = encodedToken }
                , protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(Input.Email, "Reset Your Password",
     $"Click this link to reset your password: <a href='{resetLink}'>Reset Password</a>");

            TempData["SuccessMessage"] = "A password reset link has been sent to your email.";

            return RedirectToPage("/Auth/ForgotPassword");
        }
    }
}
