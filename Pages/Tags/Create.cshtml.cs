using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Services;

namespace PRN_MANGA_PROJECT.Pages.Tags
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ITagService _tagService;

        public CreateModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        [BindProperty]
        public Tag Tag { get; set; } = new Tag();

        public bool IsCreated { get; set; } = false;

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            // ✅ Step 1: Validate required fields manually
            if (string.IsNullOrWhiteSpace(Tag.Name))
                ModelState.AddModelError("Tag.Name", "Name is required.");

            if (string.IsNullOrWhiteSpace(Tag.Description))
                ModelState.AddModelError("Tag.Description", "Description is required.");

            if (string.IsNullOrWhiteSpace(Tag.Color))
                ModelState.AddModelError("Tag.Color", "Color is required.");

         
            if (!ModelState.IsValid)
                return Page();

            try
            {
                await _tagService.CreateTagAsync(Tag);
                IsCreated = true;
                ModelState.Clear();
                Tag = new Tag(); // reset form
            }
            catch (InvalidOperationException ex)
            {
                // ✅ hiển thị thông báo lỗi trùng tên
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return Page();
        }

    }
}
