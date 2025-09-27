using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories.Auth
{
    public interface IUserRepository : IBaseRepository<User>
    {
        public Task<User> GetAnAccount(string username);
        public Task<bool> CheckPassword(User user, string password);

    }
}
