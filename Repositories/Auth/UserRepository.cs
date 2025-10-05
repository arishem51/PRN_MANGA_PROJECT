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

        public async Task AddRoleForUser(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user , roleName);
        }


        public async Task<bool> CheckEmailConfirmation(User user)
        {
            return await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<bool> CheckPassword(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> ConfirmEmail(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<IdentityResult> CreateUser(User user, string password)
        {
            return await _userManager.CreateAsync(user,password);
            
        }

        public async Task<bool> FindByEmail(string email)
        {
            return await _userManager.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> FindByPhoneNumber(string phoneNumber)
        {
            return await _userManager.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }


        public async Task<User> FindUserById(string UserId)
        {
            return await _userManager.FindByIdAsync(UserId);
        }

        public async Task<string> GenerateToken(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<User> GetAnAccount(string username)
        {
           return await _userManager.FindByNameAsync(username);
        }


        
    }
}
