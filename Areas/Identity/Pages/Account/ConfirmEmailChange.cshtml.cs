// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Areas.Identity.Pages.Account
{
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHubContext<ChangeEmailHub> _hub;

        public ConfirmEmailChangeModel(UserManager<User> userManager, SignInManager<User> signInManager ,
            IHubContext<ChangeEmailHub> hub)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hub = hub;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Your email has been changed successfully. Please log in again.";
                return RedirectToPage("/Auth/Login");
            }


            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                StatusMessage = "Error changing email.";
                return RedirectToPage("/Public/Error");
            }

            // In our UI email and user name are one and the same, so when we update the email
            // we need to update the user name.
            var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                StatusMessage = "Error changing user name.";
                return RedirectToPage("/Public/Error");
            }
            user.UserName = user.Email;
            user.NormalizedUserName = user.Email.ToUpper();
            await _userManager.UpdateAsync(user);
            var context = HttpContext.RequestServices.GetService<ApplicationDbContext>();
            var userLogin = context.UserLogins.FirstOrDefault(l => l.UserId == user.Id);

            if (userLogin != null)
            {
                userLogin.ProviderDisplayName = user.Email;
                context.UserLogins.Update(userLogin);
                await context.SaveChangesAsync();
            }

            var existingLogins = await _userManager.GetLoginsAsync(user);
            var oldGoogleLogin = existingLogins.FirstOrDefault(l => l.LoginProvider == "Google");
            if (oldGoogleLogin != null)
            {
                await _userManager.RemoveLoginAsync(user, oldGoogleLogin.LoginProvider, oldGoogleLogin.ProviderKey);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Thank you for confirming your email change.";
            await _hub.Clients.All.SendAsync("LoadChangePassword");
            return RedirectToPage("/Index");
        }
    }
}
