using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Models;
using Movies.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users.", error = ex.Message });
            }
        }

    
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user.", error = ex.Message });
            }
        }

      
        [HttpGet("deleted")]
        public async Task<ActionResult<List<User>>> GetDeletedUsers()
        {
            try
            {
                var users = await _userService.GetDeletedUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving deleted users.", error = ex.Message });
            }
        }

       
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = new User
                {
                    Username = userDto.Username,
                    Name = userDto.Name,
                    PhoneNumber = userDto.PhoneNumber,
                    Password = userDto.Password,
                    RoleId = userDto.RoleId
                };

                var createdUser = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the user.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != userDto.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in request body." });
            }

            try
            {
                var user = new User
                {
                    Id = userDto.Id,
                    Username = userDto.Username,
                    Name = userDto.Name,
                    PhoneNumber = userDto.PhoneNumber,
                    Password = userDto.Password ?? string.Empty,
                    RoleId = userDto.RoleId
                };

                var updatedUser = await _userService.UpdateUserAsync(user);
                if (updatedUser == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(updatedUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user.", error = ex.Message });
            }
        }

      
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    if (currentUserId == id)
                    {
                        return BadRequest(new { message = "You cannot delete your own account." });
                    }
                }

                var deleted = await _userService.DeleteUserAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(new { message = $"User with ID {id} has been deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }
        }

        
        [HttpPost("restore/{id}")]
        public async Task<ActionResult> RestoreUser(int id)
        {
            try
            {
                var restored = await _userService.RestoreUserAsync(id);
                if (!restored)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(new { message = $"User with ID {id} has been restored." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while restoring the user.", error = ex.Message });
            }
        }

        [HttpPost("{id}/change-password")]
        public async Task<ActionResult> ChangeUserPassword(int id, [FromBody] ChangePasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _userService.ChangePasswordAsync(id, passwordDto.NewPassword);
                if (!success)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing the password.", error = ex.Message });
            }
        }

     
        [HttpGet("profile")]
        [Authorize] // Available to all authenticated users
        public async Task<ActionResult<User>> GetProfile()
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

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user profile.", error = ex.Message });
            }
        }
    }

    // DTOs for user operations
    public class CreateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }
    }

    public class UpdateUserDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        // Optional - if empty, password won't be updated
        [MinLength(6)]
        public string? Password { get; set; }

        [Required]
        public int RoleId { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}