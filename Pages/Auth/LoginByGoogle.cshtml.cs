using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Pages.Auth
{
    public class LoginByGoogleModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public LoginByGoogleModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetCallbackAsync(bool rememberMe, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToPage("./Login");
            }

            //get info from claims of GG , middleware temporary save 
            /*
             info.LoginProvider   // "Google"
             info.ProviderKey     // ID duy nhất của user trên Google
             info.Principal  
             */
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login");
            }


            //check email linked with account db 
            //(search in aspnetuserlogins) check provider + providerKey ard used by user ?
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: rememberMe);
            if (result.Succeeded)
            {
                var checkRoleUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (checkRoleUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(checkRoleUser);

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToPage("/Index", new { area = "Admin" });
                    }
                    else
                    {
                        return RedirectToPage("/Public/HomePage");
                    }
                }

                TempData["GoogleLoginError"] = "Something Went Wrong";
                return RedirectToPage("/Auth/Login");
            }
            //get email from claim
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                // check email is exist
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    TempData["GoogleLoginError"] = "This Google account is not registered in the system.";
                    return RedirectToPage("/Auth/Login");
                }

                //if email exist create link between gg and account
                //create new table in aspnetuserlogins
                var addLoginResult = await _userManager.AddLoginAsync(user, info);

                //link successful => login
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: rememberMe);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    return RedirectToPage("/Index", new { area = "Admin" });
                }
                else
                {
                    return RedirectToPage("/Public/HomePage");
                }
            }
                else
                {
                    return RedirectToPage("./Login");
                }
            }


        }
    }
