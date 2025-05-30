using System.Linq;
using Movies.DataAccess;
using Movies.Models;

namespace Movies.DataAccess.Repositories // Fixed namespace
{
    public class UsersRepository
    {
        private readonly MoviesDbContext _context;

        public UsersRepository(MoviesDbContext context)
        {
            _context = context;
        }

        public User? Authenticate(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public User? GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}