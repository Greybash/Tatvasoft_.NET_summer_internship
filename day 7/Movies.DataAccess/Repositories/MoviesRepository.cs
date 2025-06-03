using System;
using System.Collections.Generic;
using System.Linq;
using Movies.Models;
using Movies.DataAccess; // Added correct namespace

namespace Movies.DataAccess.Repositories // Fixed namespace
{
    public class MoviesRepository
    {
        private readonly MoviesDbContext _context;

        public MoviesRepository(MoviesDbContext context)
        {
            _context = context;
        }

        public Movie? GetMovieById(int id)
        {
            return _context.Movies.FirstOrDefault(m => m.Id == id);
        }

        public IEnumerable<Movie> GetAllMovies()
        {
            return _context.Movies.ToList();
        }

        public void AddMovie(Movie movie)
        {
            _context.Movies.Add(movie);
            _context.SaveChanges();
        }

        public void UpdateMovie(Movie movie)
        {
            _context.Movies.Update(movie);
            _context.SaveChanges();
        }

        public void DeleteMovie(int id)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Movie> GetMoviesByGenre(string genre)
        {
            var lowerGenre = genre.ToLower();
            return _context.Movies
                          .Where(m => m.Genre != null && m.Genre.ToLower().Contains(lowerGenre))
                          .ToList();
        }
    }
}