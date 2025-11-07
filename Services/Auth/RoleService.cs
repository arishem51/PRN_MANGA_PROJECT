
using PRN_MANGA_PROJECT.Repositories.Auth;

namespace PRN_MANGA_PROJECT.Services.Auth
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
        public async Task SeedRole()
        {
            string[] roleNames = { "Admin", "Reader", "Author"};

            foreach (var roleName in roleNames)
            {
                if (!await _roleRepository.CheckExisteRole(roleName))
                {
                    await _roleRepository.CreateRole(roleName);
                }
            }
        }
    }
}
