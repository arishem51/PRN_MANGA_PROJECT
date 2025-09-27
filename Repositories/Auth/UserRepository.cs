using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories.Auth
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly UserManager<User> _userManager;


        public UserRepository(ApplicationDbContext context , UserManager<User> userManager) : base(context)
        {
            _userManager = userManager;

        }

        public async Task<bool> CheckPassword(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<User> GetAnAccount(string username)
        {
           return await _userManager.FindByNameAsync(username);
        }
    }
}
