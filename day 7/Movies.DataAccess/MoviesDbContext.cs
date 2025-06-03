using Microsoft.EntityFrameworkCore;
using Movies.Models;

namespace Movies.DataAccess
{
    public class MoviesDbContext : DbContext
    {
        public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure User-Movie relationship (one-to-many)
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.User)
                .WithMany(u => u.Movies)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.SetNull) // When user is deleted, set movie's UserId to null
                .IsRequired(false); // UserId is optional

            // Seed default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Administrator with full access", IsActive = true },
                new Role { Id = 2, Name = "User", Description = "Regular user with limited access", IsActive = true }
            );

            // Add indexes for better performance
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // Index on Movie.UserId for better join performance
            modelBuilder.Entity<Movie>()
                .HasIndex(m => m.UserId);

            // Index on Movie.Genre for search performance
            modelBuilder.Entity<Movie>()
                .HasIndex(m => m.Genre);
        }
    }
}