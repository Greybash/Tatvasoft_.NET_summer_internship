using Microsoft.AspNetCore.Mvc;
using Movies.DataAccess;
using Movies.Services;
using Movies.Models;
using Movies.Dto; // Added missing using

namespace Movies.Controllers // Fixed namespace consistency
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MoviesDbContext _context;
        private readonly AuthService _authService;

        public AuthController(MoviesDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginReqDto login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = _context.Users.SingleOrDefault(x =>
                    x.Username == login.Email && // Using Email from DTO
                    x.Password == login.Password);

                if (user == null)
                    return Unauthorized(new { message = "Invalid credentials" });

                var token = _authService.GenerateToken(user);

                return Ok(new LoginResDto
                {
                    Email = user.Username,
                    Role = user.Role,
                    Name = user.Username,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login.", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterReqDto register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if user already exists
                var existingUser = _context.Users.SingleOrDefault(x => x.Username == register.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists with this email" });
                }

                // Create new user
                var newUser = new User
                {
                    Username = register.Email,
                    Password = register.Password, // Note: In production, hash the password!
                    Role = register.Role ?? "User" // Default to "User" if no role specified
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                // Generate token for the new user
                var token = _authService.GenerateToken(newUser);

                return Ok(new LoginResDto
                {
                    Email = newUser.Username,
                    Role = newUser.Role,
                    Name = newUser.Username,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration.", error = ex.Message });
            }
        }
    }
}