using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;

namespace Movies.Services
{
    public class UserService
    {
        private readonly MoviesDbContext _context;

        public UserService(MoviesDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Movies.Where(m => !m.IsDeleted))
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == user.Username.ToLower());

            if (existingUser != null)
            {
                throw new InvalidOperationException($"Username '{user.Username}' already exists");
            }

            // Validate role exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId && r.IsActive);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID {user.RoleId} does not exist or is inactive");
            }

            user.IsActive = true;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Load the role for the response
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            return user;
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            var existingUser = await GetUserByIdAsync(user.Id);
            if (existingUser == null)
            {
                return null;
            }

            // Check if new username conflicts with another user
            var usernameConflict = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == user.Username.ToLower() && u.Id != user.Id);

            if (usernameConflict != null)
            {
                throw new InvalidOperationException($"Username '{user.Username}' already exists");
            }

            // Validate role exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId && r.IsActive);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID {user.RoleId} does not exist or is inactive");
            }

            existingUser.Username = user.Username;
            existingUser.Name = user.Name;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.RoleId = user.RoleId;

            // Only update password if provided (not empty)
            if (!string.IsNullOrEmpty(user.Password))
            {
                existingUser.Password = user.Password;
            }

            await _context.SaveChangesAsync();

            // Reload the role
            await _context.Entry(existingUser).Reference(u => u.Role).LoadAsync();
            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // Soft delete - set IsActive to false
            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreUserAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false;
            }

            if (user.IsActive)
            {
                throw new InvalidOperationException("User is already active");
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetDeletedUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsActive)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            user.Password = newPassword;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}