using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;

namespace PRN_MANGA_PROJECT.Services.CRUD
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountViewModel>> GetAllWithRolesAsync();

        Task<User> GetById(string id);
        Task<IdentityResult> Create(string username, string email, string password, string roleName);

        Task<IdentityResult> Update(User user);
        Task<IdentityResult> Delete(string id);
    }
}
