using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories.Auth;

namespace PRN_MANGA_PROJECT.Repositories.CRUD
{
    public class AccountRepository : BaseRepository<User>, IAccountRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHubContext<AccountHub> _hubContext;
        private readonly ApplicationDbContext _context; // ✅ giữ tham chiếu DbContext

        public AccountRepository(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IHubContext<AccountHub> hubContext)
            : base(context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _hubContext = hubContext;
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.GetType().GetProperty("Roles")?.SetValue(user, roles.ToList());
            }
            return users;
        }

        public async Task<User> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.GetType().GetProperty("Roles")?.SetValue(user, roles.ToList());
            }
            return user!;
        }

        public async Task<IdentityResult> CreateAsync(User user, string password, string? roleName)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return result;

            if (!string.IsNullOrEmpty(roleName))
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return IdentityResult.Failed(roleResult.Errors.ToArray());
                }
            }

            // ✅ Lưu thay đổi vào DB (phòng trường hợp context có các entity khác)
            await _context.SaveChangesAsync();

            // ✅ Gửi tín hiệu reload cho toàn bộ client
            await _hubContext.Clients.All.SendAsync("ReloadUsers");

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // ✅ Lưu thay đổi
                await _context.SaveChangesAsync();

                // ✅ Gửi tín hiệu realtime
                await _hubContext.Clients.All.SendAsync("ReloadUsers");
            }
            return result;
        }

        public async Task<IdentityResult> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (user.GetType().GetProperty("IsActive") != null)
                {
                    user.GetType().GetProperty("IsActive")?.SetValue(user, false);
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // ✅ Lưu thay đổi
                        await _context.SaveChangesAsync();

                        // ✅ Gửi tín hiệu reload khi xoá
                        await _hubContext.Clients.All.SendAsync("ReloadUsers");
                    }
                    return result;
                }
                else
                {
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
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                // ✅ Lưu thay đổi role vào DB
                await _context.SaveChangesAsync();

                // ✅ Gửi tín hiệu reload khi đổi role
                await _hubContext.Clients.All.SendAsync("ReloadUsers");
            }
            return result;
        }
    }
}
