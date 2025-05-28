
using movies.Models;
using movies.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly MovieService _service;
        public MovieController()
        {
            _service = new MovieService();
        }

        [HttpGet]
        public ActionResult<List<Movie>> GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Movie> Get(int id)
        {
            var movie = _service.Get(id);
            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        [HttpPost]
        public ActionResult<Movie> Add(Movie movie)
        {
            _service.Add(movie);
            return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Movie movie)
        {
            if (id != movie.Id)
                return BadRequest("ID mismatch");

            var existing = _service.Get(id);
            if (existing == null)
                return NotFound();

            _service.Update(movie);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _service.Get(id);
            if (movie == null)
                return NotFound();

            _service.Delete(id);
            return NoContent();
        }
    }
}
