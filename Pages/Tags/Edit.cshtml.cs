using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages.Tags
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TagHub> _hubContext;

        public EditModel(ApplicationDbContext context, IHubContext<TagHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Tag Tag { get; set; } = default!;

        public bool IsUpdated { get; set; } = false; // ✅ flag để hiển thị thông báo

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var tag = await _context.Tags.FirstOrDefaultAsync(m => m.Id == id);
            if (tag == null)
                return NotFound();

            Tag = tag;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Attach(Tag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // ✅ Gửi tín hiệu realtime
                await _hubContext.Clients.All.SendAsync("ReloadTags");

                IsUpdated = true; // ✅ đánh dấu để hiển thị thông báo
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(Tag.Id))
                    return NotFound();
                else
                    throw;
            }

            // ✅ Không redirect về Index — giữ nguyên trang Edit
            return Page();
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
