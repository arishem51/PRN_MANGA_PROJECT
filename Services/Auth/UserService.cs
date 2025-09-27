
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
    }
}
