using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Services.Auth
{
    public interface IUserService
    {
        Task<bool> Login (string username, string password);
        Task<IdentityResult> Register (User user , string password);
        
        Task AddRole(User user , string roleName);

        Task<string> GenerateToken(User user);

        public Task<User> FindUserById(string UserId);
        public Task<IdentityResult> ConfirmEmail(User user, string token);

        public Task<bool> checkEmailConfirmation(string username);
    }
}
