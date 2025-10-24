using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Services.Auth
{
    public interface IUserService
    {
        Task<bool> Login (string username, string password , bool rememberMe);
        Task<IdentityResult> Register (User user , string password);
        
        Task AddRole(User user , string roleName);

        Task<string> GenerateToken(User user);

        public Task<User> FindUserById(string UserId);
        public Task<IdentityResult> ConfirmEmail(User user, string token);

        public Task<bool> CheckEmailConfirmation(string username);

        Task Logout();

        Task<bool> IsExistUsername(string username);
        Task<bool> IsExistEmail(string email);
        Task<bool> IsExistPhone(string phone);

        Task<User> FindByEmail(string email);
        Task<IdentityResult> ResetPassword(User user, string token, string newPassword);

        Task<string> GenerateResetPasswordToken(User user);
        Task UpdateToken(User user);

        AuthenticationProperties GetExternalAuthenticationProperties(string provider, string redirectUrl);
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<IdentityResult> CreateExternalUserAsync(User user);
        Task SignInAsync(User user);
        Task<User?> FindByEmailAsync(string email);


    }
}
