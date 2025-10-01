
using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories.Auth;

namespace PRN_MANGA_PROJECT.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;

        public UserService(IUserRepository userRepository, SignInManager<User> signInManager)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
        }

        public async Task AddRole(User user, string roleName)
        {
            await _userRepository.AddRoleForUser(user, "Reader");
        }

        public async Task<bool> checkEmailConfirmation(string username)
        {
            var user = await _userRepository.GetAnAccount(username);
            if (user == null)
            {
                return false;
            }

            return await _userRepository.CheckEmailConfirmation(user) ;

        }

        public Task<IdentityResult> ConfirmEmail(User user, string token)
        {
            return _userRepository.ConfirmEmail(user, token);
        }

        public Task<User> FindUserById(string UserId)
        {
            return _userRepository.FindUserById(UserId);
        }

        public async Task<string> GenerateToken(User user)
        {
            return await _userRepository.GenerateToken(user);
        }

        public async Task<bool> Login(string username, string password)
        {
            var user = await _userRepository.GetAnAccount(username);
            if (user != null && await _userRepository.CheckPassword(user , password))
            {
                var response = await _signInManager.PasswordSignInAsync(user, password, false, false);
                return response.Succeeded;
            }
            return false;
        }

        public async Task<IdentityResult> Register(User user, string password)
        {
            return await _userRepository.CreateUser(user, password);
        }
    }
}
