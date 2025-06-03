using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;

namespace Movies.Services
{
    public class UserMovieService
    {
        private readonly MoviesDbContext _context;

        public UserMovieService(MoviesDbContext context)
        {
            _context = context;
        }

  
        public async Task<int> GetUserMovieCountAsync(int userId)
        {
            var count = await _context.Users
                .Where(u => u.Id == userId && u.IsActive)
                .Join(_context.Movies,
                      user => user.Id,
                      movie => movie.UserId,  
                      (user, movie) => new { user, movie })
                .Where(um => !um.movie.IsDeleted)
                .CountAsync();

            return count;
        }

        public async Task<List<UserMovieCountDto>> GetAllUsersMovieCountAsync()
        {
            var userMovieCounts = await _context.Users
                .Where(u => u.IsActive)
                .GroupJoin(_context.Movies.Where(m => !m.IsDeleted),
                          user => user.Id,
                          movie => movie.UserId,  
                          (user, movies) => new UserMovieCountDto
                          {
                              UserId = user.Id,
                              Username = user.Username,
                              Role = user.Role.Name,
                              MovieCount = movies.Count()
                          })
                .Include(u => u.Role)
                .ToListAsync();

            return userMovieCounts;
        }

  
        public async Task<UserMovieDetailsDto> GetUserMovieDetailsAsync(int userId)
        {
            var userMovieDetails = await _context.Users
                .Where(u => u.Id == userId && u.IsActive)
                .Select(u => new UserMovieDetailsDto
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Role = u.Role.Name,
                    Movies = u.Movies.Where(m => !m.IsDeleted).Select(m => new MovieSummaryDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Director = m.Director,
                        Year = m.Year,
                        Genre = m.Genre,
                        Price = m.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return userMovieDetails;
        }

       
        public async Task<List<RoleMovieStatsDto>> GetMovieStatsByRoleAsync()
        {
            var roleStats = await _context.Roles
                .Where(r => r.IsActive)
                .Select(r => new RoleMovieStatsDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    UserCount = r.Users.Count(u => u.IsActive),
                    TotalMovies = r.Users.Where(u => u.IsActive)
                                       .SelectMany(u => u.Movies)
                                       .Count(m => !m.IsDeleted),
                    AverageMoviesPerUser = r.Users.Where(u => u.IsActive).Count() > 0
                        ? (double)r.Users.Where(u => u.IsActive)
                                       .SelectMany(u => u.Movies)
                                       .Count(m => !m.IsDeleted) / r.Users.Count(u => u.IsActive)
                        : 0
                })
                .ToListAsync();

            return roleStats;
        }

     
        public async Task<bool> AssignMovieToUserAsync(int userId, int movieId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == movieId && !m.IsDeleted);

            if (user == null || movie == null)
                return false;

            movie.UserId = userId;
            await _context.SaveChangesAsync();
            return true;
        }

        
        public async Task<bool> RemoveMovieFromUserAsync(int userId, int movieId)
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == movieId && m.UserId == userId && !m.IsDeleted);

            if (movie == null)
                return false;

            movie.UserId = null; 
            await _context.SaveChangesAsync();
            return true;
        }
    }

  
    public class UserMovieCountDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int MovieCount { get; set; }
    }

    public class UserMovieDetailsDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<MovieSummaryDto> Movies { get; set; } = new List<MovieSummaryDto>();
        public int TotalMovies => Movies.Count;
    }

    public class MovieSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class RoleMovieStatsDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int TotalMovies { get; set; }
        public double AverageMoviesPerUser { get; set; }
    }
}