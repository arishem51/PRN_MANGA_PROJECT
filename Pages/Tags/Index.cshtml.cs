using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Pages.Tags
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TagHub> _hubContext;

        public IndexModel(ApplicationDbContext context, IHubContext<TagHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IList<Tag> Tags { get; set; } = new List<Tag>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 5; // ✅ mỗi trang 5 bản ghi
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Tags.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(t => t.Name.Contains(SearchTerm));
            }

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            Tags = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeactivateAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            tag.IsActive = false;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReloadTags");
            TempData["SuccessMessage"] = $"Đã ẩn thể loại: {tag.Name}";
            return RedirectToPage(new { PageNumber, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            tag.IsActive = true;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReloadTags");
            TempData["SuccessMessage"] = $"Đã kích hoạt thể loại: {tag.Name}";
            return RedirectToPage(new { PageNumber, SearchTerm });
        }
    }
}
