using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Services.HistoryService;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN_MANGA_PROJECT.Pages.Public.Manga
{
    public class ReadingHistoryModel : PageModel
    {
        private readonly IReadingHistoryService _service;

        public List<ReadingHistory> readingHistories = new List<ReadingHistory>();
        private const int PageSize = 2;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public ReadingHistoryModel(IReadingHistoryService service)
        {
            _service = service;
        }

        public async Task<IActionResult> OnGet(int pageNumber = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null) {
                int totalRecords = await _service.TotalHistory(userId);
                TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);
                CurrentPage = pageNumber;


                readingHistories = await _service.GetAllHistoryAsync(userId , pageNumber , PageSize);
            }
            else
            {
                return RedirectToPage("/Auth/Login");
            }
            return Page();  
        }

        public async Task<IActionResult> OnGetReloadAsync(int pageNumber = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Bạn cần đăng nhập để xem lịch sử đọc."
                });
            }
            int totalRecords = await _service.TotalHistory(userId);
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);
            CurrentPage = pageNumber;
            var histories = await _service.GetAllHistoryAsync(userId, pageNumber, PageSize);
            var data = histories.Select(h => new
            {
                MangaId = h.MangaId,
                MangaTitle = h.Manga?.Title,
                MangaImage = h.Manga?.CoverImageUrl,
                ChapterId = h.ChapterId,
                ChapterNumber = h.Chapter?.ChapterNumber,
                ReadAt = h.ReadAt.ToString("dd/MM/yyyy")
            }).ToList();

            return new JsonResult(new
            {
                success = true,
                data,
                totalPages = TotalPages,
                currentPage = CurrentPage,
            });
        }



    }
}
