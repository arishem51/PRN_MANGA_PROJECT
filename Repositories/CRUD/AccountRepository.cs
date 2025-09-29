using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories.Auth;

namespace PRN_MANGA_PROJECT.Repositories.CRUD
{
    public class AccountRepository : BaseRepository<User>, IAccountRepository
    {
        private readonly UserManager<User> _userManager;

        public AccountRepository(ApplicationDbContext context, UserManager<User> userManager) : base(context)
        {
            _userManager = userManager;

        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return _userManager.Users.ToList();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<IdentityResult> CreateAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> UpdateAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                return await _userManager.DeleteAsync(user);
            }
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

     
    }
}
