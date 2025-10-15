using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN_MANGA_PROJECT.Areas.Admin.Pages.Manga
{
    public class CreateModel : PageModel
    {
        public void OnGet()
        {
            // Initialize page data if needed
        }

        public IActionResult OnPost()
        {
            // Handle form submission when implemented
            return Page();
        }
    }
}
