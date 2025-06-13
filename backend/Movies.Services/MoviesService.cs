using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Movies.Models;
using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;


namespace Movies.Services
{
    public class MovieService
    {
        private readonly MoviesDbContext _context;

        public MovieService(MoviesDbContext context)
        {
            _context = context;
        }

        public List<Movie> GetAll()
        {
            return _context.Movies.Where(m => !m.IsDeleted).ToList();
        }

        public Movie Getr(int id)
        {
            return _context.Movies.Find(id);
        }
        public Movie Get(int id)
        {
            var movie = _context.Movies.Find(id);
            return (movie != null && !movie.IsDeleted) ? movie : null;
        }

        public void Add(Movie movie)
        {
            _context.Movies.Add(movie);
            _context.SaveChanges();
        }

        public void Update(Movie movie)
        {
            _context.Entry(movie).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie != null && !movie.IsDeleted)
            {
                movie.IsDeleted = true;
                _context.SaveChanges();
            }
        }
        public List<Movie> GetByGenre(string genre)
        {
            var lowerGenre = genre.ToLower();
            return _context.Movies
                .Where(m => !m.IsDeleted && m.Genre != null && m.Genre.ToLower().Contains(lowerGenre))
                .ToList();
        }
        public void Restore(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie != null && movie.IsDeleted)
            {
                movie.IsDeleted = false;
                _context.SaveChanges();
            }
        }

    }
}
