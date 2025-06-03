using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;

namespace Movies.Services
{
    public class RoleService
    {
        private readonly MoviesDbContext _context;

        public RoleService(MoviesDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower() && r.IsActive);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            var existingRole = await GetRoleByNameAsync(role.Name);
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role '{role.Name}' already exists");
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role?> UpdateRoleAsync(Role role)
        {
            var existingRole = await GetRoleByIdAsync(role.Id);
            if (existingRole == null)
            {
                return null;
            }

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;
            existingRole.IsActive = role.IsActive;

            await _context.SaveChangesAsync();
            return existingRole;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await GetRoleByIdAsync(id);
            if (role == null)
            {
                return false;
            }

            // Check if any users are assigned to this role
            var usersWithRole = await _context.Users.CountAsync(u => u.RoleId == id);
            if (usersWithRole > 0)
            {
                throw new InvalidOperationException("Cannot delete role that is assigned to users");
            }

            role.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }
    }
}