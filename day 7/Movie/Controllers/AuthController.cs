using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Services;
using Movies.Models;
using Movies.Dto;

namespace Movies.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly MoviesDbContext _context;
        private readonly AuthService _authService;
        private readonly RoleService _roleService;

        public AuthController(MoviesDbContext context, AuthService authService, RoleService roleService)
        {
            _context = context;
            _authService = authService;
            _roleService = roleService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReqDto login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .SingleOrDefaultAsync(x =>
                        x.Username == login.Email &&
                        x.Password == login.Password &&
                        x.IsActive);

                if (user == null || !user.Role.IsActive)
                    return Unauthorized(new { message = "Invalid credentials" });

                var token = _authService.GenerateToken(user);

                return Ok(new LoginResDto
                {
                    Email = user.Username,
                    Role = user.Role.Name,
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
        public async Task<IActionResult> Register([FromBody] RegisterReqDto register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingUser = await _context.Users.SingleOrDefaultAsync(x => x.Username == register.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists with this email" });
                }

                // Get role by name or default to "User"
                var roleName = register.Role ?? "User";
                var role = await _roleService.GetRoleByNameAsync(roleName);

                if (role == null)
                {
                    return BadRequest(new { message = $"Role '{roleName}' does not exist" });
                }

                var newUser = new User
                {
                    Username = register.Email,
                    Password = register.Password, // In production, hash this password
                    RoleId = role.Id,
                    IsActive = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Load the role for token generation
                await _context.Entry(newUser).Reference(u => u.Role).LoadAsync();

                var token = _authService.GenerateToken(newUser);

                return Ok(new LoginResDto
                {
                    Email = newUser.Username,
                    Role = newUser.Role.Name,
                    Name = newUser.Username,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration.", error = ex.Message });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(roles.Select(r => new { r.Id, r.Name, r.Description }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving roles.", error = ex.Message });
            }
        }
    }
}