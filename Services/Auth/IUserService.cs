namespace PRN_MANGA_PROJECT.Services.Auth
{
    public interface IUserService
    {
        Task<bool> Login (string username, string password);
        
    }
}
