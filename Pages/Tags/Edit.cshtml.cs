using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Services;

namespace PRN_MANGA_PROJECT.Pages.Tags
{
    public class EditModel : PageModel
    {
        private readonly ITagService _tagService;

        public EditModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        [BindProperty]
        public Tag Tag { get; set; } = default!;

        public bool IsUpdated { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var tagVM = await _tagService.GetTagWithMangaCountAsync(id.Value);
            if (tagVM == null)
                return NotFound();

            // Map sang entity (hoặc có thể làm ViewModel riêng)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var updated = await _tagService.UpdateTagAsync(Tag);
                if (updated == null)
                    return NotFound();

                IsUpdated = true;
                ModelState.Clear();
            }
            catch (InvalidOperationException ex)
            {
                // Trùng tên thể loại
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return Page();
        }
    }
}
