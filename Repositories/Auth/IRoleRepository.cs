namespace PRN_MANGA_PROJECT.Repositories.Auth
{
    public interface IRoleRepository
    {
        Task<bool> CheckExisteRole(string roleName);
        Task CreateRole(string roleName);

    }
}
