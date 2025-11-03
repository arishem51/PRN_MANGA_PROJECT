// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHubContext<ProfileHub> _hub;
        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IHubContext<ProfileHub> hub)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hub = hub;
        }

       
        public string Username { get; set; }

        
        [TempData]
        public string StatusMessage { get; set; }

      
        [BindProperty]
        public InputModel Input { get; set; }

     
        public static ValidationResult? ValidateBirthDate(DateTime? birthDate, ValidationContext context)
        {
            if (birthDate == null)
            {
                return new ValidationResult("Birth date is required.");
            }

            if (birthDate > DateTime.Today)
            {
                return new ValidationResult("Birth date cannot be in the future.");
            }

            if (birthDate < new DateTime(1900, 1, 1))
            {
                return new ValidationResult("Birth date is not valid.");
            }

            return ValidationResult.Success;
        }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Display(Name = "Gender")]
            public bool Gender { get; set; }

            [Display(Name = "Address")]
            public string Address { get; set; }

            [Display(Name = "Birth date")]
            [DataType(DataType.Date)]
            [Required(ErrorMessage = "Please enter your birth date.")]
            [CustomValidation(typeof(IndexModel), nameof(ValidateBirthDate))]
            public DateTime? BirthDate { get; set; }

            [Display(Name = "Created At")]
            public DateTime CreatedAt { get; set; }

            [Display(Name = "Last Login At")]
            public DateTime? LastLoginAt { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Address = user.Address,
                BirthDate = user.BirthDate
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Cập nhật các trường mới
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Gender = Input.Gender;
            user.Address = Input.Address;
            user.BirthDate = Input.BirthDate;

            // Cập nhật số điện thoại nếu thay đổi
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            else
            {
                // Nếu không thay đổi số điện thoại, vẫn đảm bảo dữ liệu khác được update
                user.PhoneNumber = Input.PhoneNumber;
            }

            // Gọi UpdateAsync để lưu thay đổi vào DB
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            await _hub.Clients.All.SendAsync("LoadProfile");
            return RedirectToPage();
        }


    }
}

