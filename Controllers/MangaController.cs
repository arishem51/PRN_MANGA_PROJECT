using Microsoft.AspNetCore.Mvc;
using PRN_MANGA_PROJECT.Services;

namespace PRN_MANGA_PROJECT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MangaController : ControllerBase
    {
        private readonly IMangaService _mangaService;

        public MangaController(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllManga()
        {
            var mangas = await _mangaService.GetAllMangaAsync();
            return Ok(mangas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMangaById(int id)
        {
            var manga = await _mangaService.GetMangaByIdAsync(id);
            if (manga == null)
                return NotFound();

            return Ok(manga);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchManga([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            var mangas = await _mangaService.SearchMangaAsync(searchTerm);
            return Ok(mangas);
        }

        [HttpGet("tag/{tagId}")]
        public async Task<IActionResult> GetMangaByTag(int tagId)
        {
            var mangas = await _mangaService.GetMangaByTagAsync(tagId);
            return Ok(mangas);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetMangaByStatus(string status)
        {
            var mangas = await _mangaService.GetMangaByStatusAsync(status);
            return Ok(mangas);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularManga([FromQuery] int count = 10)
        {
            var mangas = await _mangaService.GetPopularMangaAsync(count);
            return Ok(mangas);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentManga([FromQuery] int count = 10)
        {
            var mangas = await _mangaService.GetRecentMangaAsync(count);
            return Ok(mangas);
        }

        [HttpPost]
        public async Task<IActionResult> CreateManga([FromBody] Models.ViewModels.MangaViewModel manga)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdManga = await _mangaService.CreateMangaAsync(manga);
            return CreatedAtAction(nameof(GetMangaById), new { id = createdManga.Id }, createdManga);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateManga(int id, [FromBody] Models.ViewModels.MangaViewModel manga)
        {
            if (id != manga.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedManga = await _mangaService.UpdateMangaAsync(manga);
                return Ok(updatedManga);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManga(int id)
        {
            await _mangaService.DeleteMangaAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/bookmark")]
        public async Task<IActionResult> BookmarkManga(int id, [FromBody] string userId)
        {
            var result = await _mangaService.BookmarkMangaAsync(userId, id);
            if (!result)
                return BadRequest("Manga is already bookmarked");

            return Ok();
        }

        [HttpDelete("{id}/bookmark")]
        public async Task<IActionResult> RemoveBookmark(int id, [FromBody] string userId)
        {
            var result = await _mangaService.RemoveBookmarkAsync(userId, id);
            if (!result)
                return BadRequest("Bookmark not found");

            return Ok();
        }

        [HttpGet("{id}/bookmark")]
        public async Task<IActionResult> IsBookmarked(int id, [FromQuery] string userId)
        {
            var isBookmarked = await _mangaService.IsBookmarkedAsync(userId, id);
            return Ok(new { isBookmarked });
        }
    }
}
