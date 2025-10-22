using Microsoft.AspNetCore.Mvc;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Services;

namespace PRN_MANGA_PROJECT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public ChapterController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        /// <summary>
        /// Get all chapters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChapterViewModel>>> GetAllChapters()
        {
            try
            {
                var chapters = await _chapterService.GetAllChaptersAsync();
                return Ok(chapters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get chapter by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ChapterViewModel>> GetChapter(int id)
        {
            try
            {
                var chapter = await _chapterService.GetChapterByIdAsync(id);
                if (chapter == null)
                {
                    return NotFound($"Chapter with ID {id} not found.");
                }
                return Ok(chapter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get chapter with images by ID
        /// </summary>
        [HttpGet("{id}/with-images")]
        public async Task<ActionResult<ChapterViewModel>> GetChapterWithImages(int id)
        {
            try
            {
                var chapter = await _chapterService.GetChapterWithImagesAsync(id);
                if (chapter == null)
                {
                    return NotFound($"Chapter with ID {id} not found.");
                }
                return Ok(chapter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all chapters by manga ID
        /// </summary>
        [HttpGet("manga/{mangaId}")]
        public async Task<ActionResult<IEnumerable<ChapterViewModel>>> GetChaptersByMangaId(int mangaId)
        {
            try
            {
                var chapters = await _chapterService.GetChaptersByMangaIdAsync(mangaId);
                return Ok(chapters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get chapters by manga ID with pagination
        /// </summary>
        [HttpGet("manga/{mangaId}/paged")]
        public async Task<ActionResult<PagedResult<ChapterViewModel>>> GetChaptersByMangaIdPaged(
            int mangaId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var paginationParams = new PaginationParams
                {
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _chapterService.GetChaptersByMangaIdPagedAsync(mangaId, paginationParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get next chapter
        /// </summary>
        [HttpGet("{id}/next")]
        public async Task<ActionResult<ChapterViewModel>> GetNextChapter(int id, [FromQuery] int mangaId)
        {
            try
            {
                var chapter = await _chapterService.GetNextChapterAsync(mangaId, id);
                if (chapter == null)
                {
                    return NotFound("No next chapter found.");
                }
                return Ok(chapter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get previous chapter
        /// </summary>
        [HttpGet("{id}/previous")]
        public async Task<ActionResult<ChapterViewModel>> GetPreviousChapter(int id, [FromQuery] int mangaId)
        {
            try
            {
                var chapter = await _chapterService.GetPreviousChapterAsync(mangaId, id);
                if (chapter == null)
                {
                    return NotFound("No previous chapter found.");
                }
                return Ok(chapter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new chapter
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ChapterViewModel>> CreateChapter([FromBody] CreateChapterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var chapter = await _chapterService.CreateChapterAsync(model);
                return CreatedAtAction(nameof(GetChapter), new { id = chapter.Id }, chapter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing chapter
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ChapterViewModel>> UpdateChapter(int id, [FromBody] UpdateChapterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var chapterExists = await _chapterService.ChapterExistsAsync(id);
                if (!chapterExists)
                {
                    return NotFound($"Chapter with ID {id} not found.");
                }

                var chapter = await _chapterService.UpdateChapterAsync(id, model);
                return Ok(chapter);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a chapter (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteChapter(int id)
        {
            try
            {
                var result = await _chapterService.DeleteChapterAsync(id);
                if (!result)
                {
                    return NotFound($"Chapter with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
