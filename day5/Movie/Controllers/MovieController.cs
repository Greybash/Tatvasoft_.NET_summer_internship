using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Movies.Services;
using Movies.Models;
using Microsoft.AspNetCore.Authorization;


namespace Movies.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MovieService _movieService;

        public MoviesController(MovieService movieService)
        {
            _movieService = movieService;
        }

        
        [HttpGet]
        public ActionResult<List<Movie>> GetAll()
        {
            try
            {
                var movies = _movieService.GetAll();
                return Ok(movies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving movies.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Movie> Get(int id)
        {
            try
            {
                var movie = _movieService.Get(id);
                if (movie == null)
                {
                    return NotFound(new { message = $"Movie with ID {id} not found." });
                }
                return Ok(movie);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the movie.", error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult<Movie> Add([FromBody] Movie movie)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _movieService.Add(movie);
                return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the movie.", error = ex.Message });
            }
        }

        
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Movie movie)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != movie.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in request body." });
            }

            try
            {
                var existingMovie = _movieService.Get(id);
                if (existingMovie == null)
                {
                    return NotFound(new { message = $"Movie with ID {id} not found." });
                }

                _movieService.Update(movie);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the movie.", error = ex.Message });
            }
        }

       
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                var movie = _movieService.Get(id);
                if (movie == null)
                {
                    return NotFound(new { message = $"Movie with ID {id} not found." });
                }

                _movieService.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the movie.", error = ex.Message });
            }
        }

        
        [HttpGet("search")]
        public ActionResult<List<Movie>> SearchByGenre([FromQuery] string genre)
        {
            if (string.IsNullOrWhiteSpace(genre))
            {
                return BadRequest(new { message = "Genre query parameter is required." });
            }

            try
            {
                var movies = _movieService.GetByGenre(genre);
                if (movies == null || movies.Count == 0)
                {
                    return NotFound(new { message = $"No movies found with genre containing '{genre}'." });
                }
                return Ok(movies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching for movies.", error = ex.Message });
            }
        }

        [HttpPost("restore/{id}")]
        public ActionResult Restore(int id)
        {
            try
            {
                var movie = _movieService.Getr(id);
                if (movie == null)
                {
                    return NotFound(new { message = $"Movie with ID {id} not found or is not deleted." });
                }

                if (!movie.IsDeleted)
                {
                    return BadRequest(new { message = $"Movie with ID {id} is not deleted." });
                }

                _movieService.Restore(id);
                return Ok(new { message = $"Movie with ID {id} has been restored." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while restoring the movie.", error = ex.Message });
            }
        }


    }
}