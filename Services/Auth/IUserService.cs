using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Services.Auth
{
    public interface IUserService
    {
        Task<bool> Login (string username, string password);
        Task<IdentityResult> Register (User user , string password);
        
    }
}
