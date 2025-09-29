using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Pages
{
    public class ViewAccountModel : PageModel
    {
        public void OnGet()
        {
            Users = _userManager.Users.ToList();
        }
        private readonly UserManager<User> _userManager;

        public ViewAccountModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public List<User> Users { get; set; } = new List<User>();

       
    }
}
