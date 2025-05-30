using Microsoft.EntityFrameworkCore;
using Movies.Models;

namespace Movies.DataAccess // Fixed namespace consistency
{
    public class MoviesDbContext : DbContext
    {
        public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
