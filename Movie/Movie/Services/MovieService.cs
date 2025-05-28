using movies.Models;
using System.Collections.Generic;
using System.Linq;

namespace movies.Services
{
    public class MovieService
    {
        private static List<Movie> movies = new List<Movie>
        {
            new Movie { Id = 1, Title = "Inception", Director = "Christopher Nolan", Year = 2010, Price = 14.99m, Genre = "Sci-Fi" },
            new Movie { Id = 2, Title = "The Godfather", Director = "Francis Ford Coppola", Year = 1972, Price = 19.99m, Genre = "Crime" }
        };

        public List<Movie> GetAll() => movies;

        public Movie Get(int id) => movies.FirstOrDefault(m => m.Id == id);

        public void Add(Movie movie)
        {
            movie.Id = movies.Count == 0 ? 1 : movies.Max(m => m.Id) + 1;
            movies.Add(movie);
        }

        public void Update(Movie movie)
        {
            var existing = Get(movie.Id);
            if (existing != null)
            {
                existing.Title = movie.Title;
                existing.Director = movie.Director;
                existing.Year = movie.Year;
                existing.Price = movie.Price;
                existing.Genre = movie.Genre;
            }
        }

        public void Delete(int id)
        {
            var movie = Get(id);
            if (movie != null)
                movies.Remove(movie);
        }
    }
}
