using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Models.ViewModels.CRUD;
using PRN_MANGA_PROJECT.Repositories.CRUD;

namespace PRN_MANGA_PROJECT.Services.CRUD
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHubContext<AccountHub> _hubContext;

        public AccountService(
            IAccountRepository repo,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IHubContext<AccountHub> hubContext)
        {
            _repo = repo;
            _userManager = userManager;
            _roleManager = roleManager;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<AccountViewModel>> GetAllWithRolesAsync()
        {
            var users = await _repo.GetAllAsync();
            var list = new List<AccountViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var role = roles.FirstOrDefault() ?? "(Chưa gán)";

                if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    continue;
                list.Add(new AccountViewModel
                {
                    Id = u.Id,
                    Username = u.UserName ?? "",
                    Email = u.Email ?? "",
                    Role = role,
                    IsActive = u.IsActive
                });
            }
            return list;
        }

        public async Task<User?> GetById(string id) => await _repo.GetByIdAsync(id);

        public async Task<IdentityResult> Create(string username, string email, string password, string roleName)
        {
            var user = new User
            {
                UserName = username,
                Email = email,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return result;

            await AssignRoleAsync(user, roleName);
            await _hubContext.Clients.All.SendAsync("ReloadUsers");

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> Update(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                await _hubContext.Clients.All.SendAsync("ReloadUsers");
            return result;
        }

        public async Task<IdentityResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                await _hubContext.Clients.All.SendAsync("ReloadUsers");

            return result;
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
                await _hubContext.Clients.All.SendAsync("ReloadUsers");

            return result;
        }
    }
}
