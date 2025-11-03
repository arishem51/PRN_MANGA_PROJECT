using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Services;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages.Tags
{
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
