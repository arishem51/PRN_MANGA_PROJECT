using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Services;

namespace PRN_MANGA_PROJECT.Pages.Tags
{
    public class DeleteModel : PageModel
    {
        private readonly ITagService _tagService;

        public DeleteModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        [BindProperty]
        public Tag Tag { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var tagVM = await _tagService.GetTagWithMangaCountAsync(id.Value);
            if (tagVM == null)
                return NotFound();

            // ánh xạ tạm sang entity để hiển thị view
            Tag = new Tag
            {
                Id = tagVM.Id,
                Name = tagVM.Name,
                Description = tagVM.Description,
                Color = tagVM.Color,
                IsActive = tagVM.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var success = await _tagService.DeleteTagAsync(id.Value);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy thể loại cần xóa.");
                return Page();
            }

            // ✅ trở về trang danh sách
            return RedirectToPage("./Index");
        }
    }
}
