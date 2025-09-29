using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Services.CRUD
{
    public interface IAccountService
    {
        Task<IEnumerable<User>> GetAll();
        Task<User> GetById(string id);
        Task<IdentityResult> Create(string username, string email, string password);
        Task<IdentityResult> Update(User user);
        Task<IdentityResult> Delete(string id);
    }
}
