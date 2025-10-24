
using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Repositories.Auth
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleRepository(RoleManager<IdentityRole> roleManager) 
        { 
            _roleManager = roleManager;
        }
        public async Task<bool> CheckExisteRole(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task CreateRole(string roleName)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

    }
}
