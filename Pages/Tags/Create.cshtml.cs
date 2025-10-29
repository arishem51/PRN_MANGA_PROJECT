using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages.Tags
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TagHub> _hubContext;

        public CreateModel(ApplicationDbContext context, IHubContext<TagHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Tag Tag { get; set; } = new Tag();

        public bool IsCreated { get; set; } = false; // ✅ flag hiển thị thông báo

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Tags.Add(Tag);
            await _context.SaveChangesAsync();

            // ✅ Gửi tín hiệu realtime sau khi tạo
            await _hubContext.Clients.All.SendAsync("ReloadTags");

            IsCreated = true; // ✅ hiển thị thông báo
            ModelState.Clear(); // ✅ xóa dữ liệu cũ trong form
            Tag = new Tag(); // reset form

            return Page(); // ❌ không Redirect, giữ nguyên trang
        }
    }
}
