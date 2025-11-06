using Microsoft.AspNetCore.SignalR;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels;
using PRN_MANGA_PROJECT.Repositories;

namespace PRN_MANGA_PROJECT.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IHubContext<TagHub> _hubContext;
        public TagService(ITagRepository tagRepository, IHubContext<TagHub> hubContext)
        {
            _tagRepository = tagRepository;
            _hubContext = hubContext;
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
        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            // ✅ Kiểm tra trùng tên (không phân biệt hoa thường)
            var existingTag = (await _tagRepository.GetAllAsync())
                .FirstOrDefault(t => t.Name.ToLower().Trim() == tag.Name.ToLower().Trim());

            if (existingTag != null)
            {
                throw new InvalidOperationException($"Thể loại '{tag.Name}' đã tồn tại!");
            }

            await _tagRepository.AddAsync(tag);
            await _tagRepository.SaveChangesAsync();

            // ✅ Gửi tín hiệu realtime sau khi tạo
            await _hubContext.Clients.All.SendAsync("ReloadTags");

            return tag;
        }
        public async Task<Tag?> UpdateTagAsync(Tag tag)
        {
            var existingTag = await _tagRepository.GetByIdAsync(tag.Id);
            if (existingTag == null)
                return null;

            // ✅ Kiểm tra trùng tên (ngoại trừ chính nó)
            var duplicate = (await _tagRepository.GetAllAsync())
                .FirstOrDefault(t => t.Id != tag.Id &&
                                     t.Name.ToLower().Trim() == tag.Name.ToLower().Trim());
            if (duplicate != null)
                throw new InvalidOperationException($"Tên thể loại '{tag.Name}' đã tồn tại!");

            // ✅ Cập nhật dữ liệu
            existingTag.Name = tag.Name;
            existingTag.Description = tag.Description;
            existingTag.Color = tag.Color;
            existingTag.IsActive = tag.IsActive;

            await _tagRepository.UpdateAsync(existingTag);
            await _tagRepository.SaveChangesAsync();

            // ✅ Gửi tín hiệu realtime sau khi update
            await _hubContext.Clients.All.SendAsync("ReloadTags");

            return existingTag;
        }
        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
                return false;

            await _tagRepository.DeleteAsync(tag);
            await _tagRepository.SaveChangesAsync();

            // ✅ gửi tín hiệu realtime cho client cập nhật danh sách
            await _hubContext.Clients.All.SendAsync("ReloadTags");

            return true;
        }
    }
}
