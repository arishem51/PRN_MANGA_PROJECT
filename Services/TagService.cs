using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Repositories;

namespace PRN_MANGA_PROJECT.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<TagViewModel>> GetActiveTagsAsync()
        {
            var tags = await _tagRepository.GetActiveTagsAsync();
            return tags.Select(t => new TagViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Color = t.Color,
                IsActive = t.IsActive,
                MangaCount = t.MangaTags?.Count ?? 0
            });
        }

        public async Task<TagViewModel?> GetTagWithMangaCountAsync(int id)
        {
            var tag = await _tagRepository.GetTagWithMangaCountAsync(id);
            if (tag == null) return null;

            return new TagViewModel
            {
                Id = tag.Id,
                Name = tag.Name,
                Description = tag.Description,
                Color = tag.Color,
                IsActive = tag.IsActive,
                MangaCount = tag.MangaTags?.Count ?? 0
            };
        }
    }
}
