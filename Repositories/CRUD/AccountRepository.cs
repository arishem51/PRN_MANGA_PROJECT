using Microsoft.AspNetCore.Identity;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories.Auth;

namespace PRN_MANGA_PROJECT.Repositories.CRUD
{
    public class AccountRepository : BaseRepository<User>, IAccountRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountRepository(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
            : base(context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (user is not null)
                {
                    // Nếu bạn không có property Roles trong model User
                    // thì bạn có thể chỉ cần gán user.RoleName = roles.FirstOrDefault();
                    if (user.GetType().GetProperty("Roles") != null)
                    {
                        user.GetType().GetProperty("Roles")?.SetValue(user, roles.ToList());
                    }
                }
            }
            return users;
        }

        public async Task<User> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (user.GetType().GetProperty("Roles") != null)
                {
                    user.GetType().GetProperty("Roles")?.SetValue(user, roles.ToList());
                }
            }
            return user!;
        }
     

        public async Task<IdentityResult> CreateAsync(User user, string password, string? roleName)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return result;

            if (!string.IsNullOrEmpty(roleName))
            {
                // Nếu role chưa tồn tại, tạo mới
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }

                // Gán role cho user
                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    // rollback nếu gán role thất bại
                    await _userManager.DeleteAsync(user);
                    return IdentityResult.Failed(roleResult.Errors.ToArray());
                }
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // ✅ Set IsActive = false thay vì xóa
                if (user.GetType().GetProperty("IsActive") != null)
                {
                    user.GetType().GetProperty("IsActive")?.SetValue(user, false);
                    return await _userManager.UpdateAsync(user);
                }
                else
                {
                    // Nếu User không có property IsActive
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "Property 'IsActive' not found on User model."
                    });
                }
            }
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }


        public async Task<IdentityResult> AssignRoleAsync(User user, string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            return await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}
