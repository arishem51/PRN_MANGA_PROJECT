using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories.CRUD;

namespace PRN_MANGA_PROJECT.Services.CRUD
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;

        public AccountService(IAccountRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<User>> GetAll() => _repo.GetAllAsync();
        public Task<User> GetById(string id) => _repo.GetByIdAsync(id);

        public async Task<IdentityResult> Create(string username, string email, string password, string roleName)
        {
            var user = new User { UserName = username, Email = email, IsActive = true };
            return await _repo.CreateAsync(user, password, roleName);
        }

        public Task<IdentityResult> Update(User user) => _repo.UpdateAsync(user);
        public Task<IdentityResult> Delete(string id) => _repo.DeleteAsync(id);


    }
}
