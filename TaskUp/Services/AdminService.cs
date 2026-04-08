using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskUp.Data;
using TaskUp.Models;
using TaskUp.ViewModels.Admin;

namespace TaskUp.Services
{
    public class AdminService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminService(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UserWithRoleVM>> GetAllUsersWithRoles()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserWithRoleVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserWithRoleVM
                {
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    JoinedAt = user.CreatedAt.ToString("MMM dd, yyyy")
                });
            }

            return result;
        }

        public async Task<bool> MakeAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole == null) return false;

            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id);

            if (!exists)
            {
                var userRole = new IdentityUserRole<string>
                {
                    UserId = userId,
                    RoleId = adminRole.Id
                };
                
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> RemoveAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole == null) return false;

            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id);

            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> IsAdminAsync(string userId)
        {
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole == null) return false;

            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id);
        }
    }
}