using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories.Auth
{
    public interface IUserRepository : IBaseRepository<User>
    {
        public Task<User> GetAnAccount(string username);
        public Task<bool> CheckPassword(User user, string password);

        public Task<IdentityResult> CreateUser(User user , string password);

        public Task AddRoleForUser(User user, string roleName);

        public Task<string> GenerateToken(User user);
        public Task<string> GenerateResetPasswordToken(User user);

        public Task<User> FindUserById(string UserId);
        public Task<IdentityResult> ConfirmEmail(User user , string token);

        Task<bool> CheckEmailConfirmation(User user);

        Task<bool> FindByEmail(string email);

        Task<User> FindUserByEmail(string email);

        Task<bool> FindByPhoneNumber(string phoneNumber);

        Task<IdentityResult> ResetPassword(User user, string token, string newPassword);

        Task UpdateToken(User user);
    }
}
