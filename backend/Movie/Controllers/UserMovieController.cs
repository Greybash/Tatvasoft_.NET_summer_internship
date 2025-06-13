/*using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Services;
using System.Security.Claims;

namespace Movies.Controllers
{
   [ApiController]
   [Route("api/[controller]")]
   [Authorize]
   public class UserMovieController : ControllerBase
   {
       private readonly UserMovieService _userMovieService;

       public UserMovieController(UserMovieService userMovieService)
       {
           _userMovieService = userMovieService;
       }


       [HttpGet("my-movies/count")]
       public async Task<ActionResult<int>> GetMyMovieCount()
       {
           try
           {
               var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
               if (userIdClaim == null)
               {
                   return BadRequest(new { message = "User ID not found in token" });
               }

               if (!int.TryParse(userIdClaim.Value, out int userId))
               {
                   return BadRequest(new { message = "Invalid user ID format" });
               }

               var count = await _userMovieService.GetUserMovieCountAsync(userId);
               return Ok(new { userId, movieCount = count });
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while retrieving movie count.", error = ex.Message });
           }
       }


       [HttpGet("users/{userId}/movies/count")]
       [Authorize(Roles = "admin")]
       public async Task<ActionResult<int>> GetUserMovieCount(int userId)
       {
           try
           {
               var count = await _userMovieService.GetUserMovieCountAsync(userId);
               return Ok(new { userId, movieCount = count });
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while retrieving movie count.", error = ex.Message });
           }
       }






       [HttpGet("my-movies")]
       public async Task<ActionResult> GetMyMovies()
       {
           try
           {
               var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
               if (userIdClaim == null)
               {
                   return BadRequest(new { message = "User ID not found in token" });
               }

               if (!int.TryParse(userIdClaim.Value, out int userId))
               {
                   return BadRequest(new { message = "Invalid user ID format" });
               }

               var userMovies = await _userMovieService.GetUserMovieDetailsAsync(userId);
               if (userMovies == null)
               {
                   return NotFound(new { message = "User not found" });
               }

               return Ok(userMovies);
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while retrieving user movies.", error = ex.Message });
           }
       }


       [HttpGet("users/{userId}/movies")]
       [Authorize(Roles = "admin")]
       public async Task<ActionResult> GetUserMovies(int userId)
       {
           try
           {
               var userMovies = await _userMovieService.GetUserMovieDetailsAsync(userId);
               if (userMovies == null)
               {
                   return NotFound(new { message = $"User with ID {userId} not found" });
               }

               return Ok(userMovies);
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while retrieving user movies.", error = ex.Message });
           }
       }

       [HttpGet("stats/by-role")]
       [Authorize(Roles = "admin")]
       public async Task<ActionResult> GetMovieStatsByRole()
       {
           try
           {
               var roleStats = await _userMovieService.GetMovieStatsByRoleAsync();
               return Ok(roleStats);
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while retrieving movie statistics.", error = ex.Message });
           }
       }


       [HttpPost("my-movies/{movieId}")]
       public async Task<ActionResult> AssignMovieToMe(int movieId)
       {
           try
           {
               var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
               if (userIdClaim == null)
               {
                   return BadRequest(new { message = "User ID not found in token" });
               }

               if (!int.TryParse(userIdClaim.Value, out int userId))
               {
                   return BadRequest(new { message = "Invalid user ID format" });
               }

               var success = await _userMovieService.AssignMovieToUserAsync(userId, movieId);
               if (!success)
               {
                   return NotFound(new { message = "User or movie not found" });
               }

               return Ok(new { message = "Movie assigned successfully" });
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while assigning movie.", error = ex.Message });
           }
       }


       [HttpPost("users/{userId}/movies/{movieId}")]
       [Authorize(Roles = "admin")]
       public async Task<ActionResult> AssignMovieToUser(int userId, int movieId)
       {
           try
           {
               var success = await _userMovieService.AssignMovieToUserAsync(userId, movieId);
               if (!success)
               {
                   return NotFound(new { message = "User or movie not found" });
               }

               return Ok(new { message = "Movie assigned successfully" });
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while assigning movie.", error = ex.Message });
           }
       }

       [HttpDelete("my-movies/{movieId}")]
       public async Task<ActionResult> RemoveMovieFromMe(int movieId)
       {
           try
           {
               var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
               if (userIdClaim == null)
               {
                   return BadRequest(new { message = "User ID not found in token" });
               }

               if (!int.TryParse(userIdClaim.Value, out int userId))
               {
                   return BadRequest(new { message = "Invalid user ID format" });
               }

               var success = await _userMovieService.RemoveMovieFromUserAsync(userId, movieId);
               if (!success)
               {
                   return NotFound(new { message = "Movie not found or not assigned to user" });
               }

               return Ok(new { message = "Movie removed successfully" });
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while removing movie.", error = ex.Message });
           }
       }


       [HttpDelete("users/{userId}/movies/{movieId}")]
       [Authorize(Roles = "admin")]
       public async Task<ActionResult> RemoveMovieFromUser(int userId, int movieId)
       {
           try
           {
               var success = await _userMovieService.RemoveMovieFromUserAsync(userId, movieId);
               if (!success)
               {
                   return NotFound(new { message = "Movie not found or not assigned to user" });
               }

               return Ok(new { message = "Movie removed successfully" });
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { message = "An error occurred while removing movie.", error = ex.Message });
           }
       }
   }
}
*/