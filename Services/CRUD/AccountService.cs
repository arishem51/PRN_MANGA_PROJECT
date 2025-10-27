using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using PRN_MANGA_PROJECT.Repositories.CRUD;

namespace PRN_MANGA_PROJECT.Services.CRUD
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;
        private readonly UserManager<User> _userManager;

        public AccountService(IAccountRepository repo, UserManager<User> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        public async Task<IEnumerable<AccountViewModel>> GetAllWithRolesAsync()
        {
            var users = _userManager.Users.ToList();
            var list = new List<AccountViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new AccountViewModel
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Email = u.Email,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }
            return list;
        }

        public Task<User> GetById(string id) => _repo.GetByIdAsync(id);

        public async Task<IdentityResult> Create(string username, string email, string password, string roleName)
        {
            var user = new User
            {
                UserName = username,
                Email = email,
                IsActive = true,
                EmailConfirmed = true // ✅ Xác nhận email ngay khi tạo
            };

            return await _repo.CreateAsync(user, password, roleName);
        }

        public Task<IdentityResult> Update(User user) => _repo.UpdateAsync(user);
        public Task<IdentityResult> Delete(string id) => _repo.DeleteAsync(id);

        public async Task<IdentityResult> AssignRoleAsync(User user, string roleName)
        {
            return await _repo.AssignRoleAsync(user, roleName);
        }
    }
}
