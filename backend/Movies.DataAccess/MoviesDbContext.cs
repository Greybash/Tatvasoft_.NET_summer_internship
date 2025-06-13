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
        public DbSet<MissionTheme> MissionThemes { get; set; }
        public DbSet<MissionSkill> MissionSkills { get; set; }
        public DbSet<Mission> Missions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<MissionApp> MissionApps{ get; set; }
        public DbSet<UserDetail> UserDetails{ get; set; }



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

            // Configure Country entity
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).HasMaxLength(10);
                entity.Property(e => e.IsActive).IsRequired();
            });

            // Configure City entity
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).IsRequired();

                // Configure City-Country relationship
                entity.HasOne(c => c.Country)
                      .WithMany(co => co.Cities)
                      .HasForeignKey(c => c.CountryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<UserDetail>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Configure relationship with User (only once)
                entity.HasOne(ud => ud.User)
                      .WithMany(m => m.UserDetails)
                      .HasForeignKey(ud => ud.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship with Country
                entity.HasOne(ud => ud.Country)
                      .WithMany(m => m.UserDetails)  // Assuming Country doesn't have a UserDetails collection
                      .HasForeignKey(ud => ud.CountryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship with City
                entity.HasOne(ud => ud.City)
                      .WithMany(m => m.UserDetails)  // Assuming City doesn't have a UserDetails collection
                      .HasForeignKey(ud => ud.CityId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<MissionApp>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(c => c.Mission)
                     .WithMany(m => m.MissionApps)
                     .HasForeignKey(c => c.MissionId)
                     .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.User)
     .WithMany(u => u.MissionApps)
     .HasForeignKey(c => c.UserId)
     .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Mission entity
            modelBuilder.Entity<Mission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.MissionTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MissionDescription).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.MissionOrganisationName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MissionOrganisationDetail).HasMaxLength(1000);
                entity.Property(e => e.MissionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MissionSkillId).IsRequired().HasMaxLength(500);
                entity.Property(e => e.MissionImages).HasMaxLength(2000);
                entity.Property(e => e.MissionDocuments).HasMaxLength(2000);
                entity.Property(e => e.MissionAvailability).HasMaxLength(50);
                entity.Property(e => e.MissionVideoUrl).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Configure CountryId and CityId as foreign keys without navigation properties
                entity.Property(e => e.CountryId).IsRequired();
                entity.Property(e => e.CityId).IsRequired();

                // Configure Mission-MissionTheme relationship (keeping this navigation property)
                entity.HasOne(m => m.MissionTheme)
                      .WithMany()
                      .HasForeignKey(m => m.MissionThemeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure MissionTheme entity
            modelBuilder.Entity<MissionTheme>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Explicitly configure auto-increment for Id
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();

                // Configure Name property
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                // Configure IsActive property
                entity.Property(e => e.IsActive)
                      .IsRequired();
            });

            // Configure MissionSkill entity
            modelBuilder.Entity<MissionSkill>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Explicitly configure auto-increment for Id
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();

                // Configure Name property
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                // Configure IsActive property
                entity.Property(e => e.IsActive)
                      .IsRequired();
            });

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

            // Country indexes
            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Country>()
                .HasIndex(c => c.IsActive);

            // City indexes
            modelBuilder.Entity<City>()
                .HasIndex(c => new { c.Name, c.CountryId })
                .IsUnique();

            modelBuilder.Entity<City>()
                .HasIndex(c => c.CountryId);

            modelBuilder.Entity<City>()
                .HasIndex(c => c.IsActive);

            // Mission indexes for better performance
            modelBuilder.Entity<Mission>()
                .HasIndex(m => m.MissionTitle);

            modelBuilder.Entity<Mission>()
                .HasIndex(m => m.CountryId);

            modelBuilder.Entity<Mission>()
                .HasIndex(m => m.CityId);

            modelBuilder.Entity<Mission>()
                .HasIndex(m => m.MissionThemeId);

            modelBuilder.Entity<Mission>()
                .HasIndex(m => m.StartDate);

            modelBuilder.Entity<Mission>()
                .HasIndex(m => m.EndDate);

            modelBuilder.Entity<Mission>()
                .HasIndex(m => m.IsActive);

            modelBuilder.Entity<Mission>()
                .HasIndex(m => m.CreatedAt);

            // MissionTheme indexes for better performance
            modelBuilder.Entity<MissionTheme>()
                .HasIndex(mt => mt.Name)
                .IsUnique();

            modelBuilder.Entity<MissionTheme>()
                .HasIndex(mt => mt.IsActive);

            // MissionSkill indexes for better performance
            modelBuilder.Entity<MissionSkill>()
                .HasIndex(ms => ms.Name)
                .IsUnique();

            modelBuilder.Entity<MissionSkill>()
                .HasIndex(ms => ms.IsActive);
        }
    }
}