using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.DotNet.Scaffolding.Shared;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.Auth;
using PRN_MANGA_PROJECT.Services.Auth;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;


namespace PRN_MANGA_PROJECT.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public RegisterDTO Input { get; set; } = new RegisterDTO();

        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<User> signInManager;

        public RegisterModel(IUserService userService, IEmailSender emailSender , SignInManager<User> signInManager)
        {
            _userService = userService;
            _emailSender = emailSender;
            signInManager = signInManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            //validate phone number exist

            if (await _userService.IsExistPhone(Input.PhoneNumber))
            {
                ModelState.AddModelError("Input.PhoneNumber", "Phone number is existed");
            }


            //validate email exist 

            if (await _userService.IsExistEmail(Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Email is existed");
            }

            //validate username exist

            if (await _userService.IsExistUsername(Input.Username))
            {
                ModelState.AddModelError("Input.Username", "Username is existed");
            }


            if (!ModelState.IsValid)
            {
                return Page();
            }

            var newUser = new User
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                UserName = Input.Username,
                Address = Input.Address,
                Gender = Input.Gender,
                PhoneNumber = Input.PhoneNumber,
                BirthDate = Input.BirthDate,
            };

            var response = await _userService.Register(newUser, Input.Password);

            if (!response.Succeeded)
            {
                foreach (var error in response.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return Page();
            }

            await _userService.AddRole(newUser, "Reader");
            var newToken = await _userService.GenerateToken(newUser);
            var confirmationLink = Url.Page("/Auth/ConfirmEmail", pageHandler: null,
                values: new { userId = newUser.Id, token = newToken } ,
                protocol: Request.Scheme
            );
            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                   $"Hãy xác nhận email bằng cách click vào link: <a href='{confirmationLink}'>Xác nhận</a>");
            return RedirectToPage("/Auth/RegisterConfirmation");
        }

        public IActionResult OnGetGoogleLogin()
        {
            var redirectUrl = Url.Page("/Auth/Register", pageHandler: "GoogleResponse");
            var properties = _userService.GetExternalAuthenticationProperties("Google" , redirectUrl);
            return new ChallengeResult("Google", properties);
        }

        public async Task<IActionResult> OnGetGoogleResponse()
        {
            var info = await _userService.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("/Auth/Register");
            }

            var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Name);
            if(email == null)
            {
                return RedirectToPage("/Auth/Register");
            }
            var existingUser = await _userService.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var newUser = new User
                {
                    Email = email,
                    UserName = email.Split('@')[0],
                    FirstName = name.Split(' ').First(),
                    LastName = string.Join(" ", name.Split(' ').Skip(1)),
                    EmailConfirmed = true
                };

                var result = await _userService.CreateExternalUserAsync(newUser);
                if (result.Succeeded)
                {
                    await _userService.AddRole(newUser, "Reader");
                    await _userService.SignInAsync(newUser);
                    return RedirectToPage("/Index");
                }
                else
                {
                    ModelState.AddModelError("", "Cannot create user from Google login");
                    return Page();
                }
            }
            else
            {
                await _userService.SignInAsync(existingUser);
                return RedirectToPage("/Index");
            }
        }


    }
}

