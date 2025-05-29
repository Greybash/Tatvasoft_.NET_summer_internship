using Microsoft.EntityFrameworkCore;
using Movies.Models;

namespace movies.DataAccess
{
    public class MoviesDbContext : DbContext
    {
        public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
    }
}
