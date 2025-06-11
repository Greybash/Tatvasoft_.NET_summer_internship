using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Services;
using Movies.Models;
using System.Security.Claims;

namespace Movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class MissionThemeController : ControllerBase
    {
        private readonly MissionThemeService _missionThemeService;

        public MissionThemeController(MissionThemeService missionThemeService)
        {
            _missionThemeService = missionThemeService;
        }

        /// <summary>
        /// Get all mission themes with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMissionThemes(
            [FromQuery] string? name = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (themes, totalCount) = await _missionThemeService.GetAllMissionThemesAsync(
                    name, isActive, sortBy, sortDescending, page, pageSize);

                var response = new
                {
                    Data = themes,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving mission themes.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get mission theme by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMissionTheme(int id)
        {
            try
            {
                var theme = await _missionThemeService.GetMissionThemeByIdAsync(id);

                if (theme == null)
                    return NotFound(new { message = $"Mission theme with ID {id} not found." });

                return Ok(theme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the mission theme.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new mission theme (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateMissionTheme([FromBody] MissionTheme request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if theme name already exists
                if (await _missionThemeService.MissionThemeNameExistsAsync(request.Name))
                {
                    return BadRequest(new { message = "A mission theme with this name already exists." });
                }

                var createdTheme = await _missionThemeService.CreateMissionThemeAsync(request, userId.Value);

                return CreatedAtAction(
                    nameof(GetMissionTheme),
                    new { id = createdTheme.Id },
                    createdTheme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the mission theme.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing mission theme (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateMissionTheme(int id, [FromBody] MissionTheme request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if theme exists
                if (!await _missionThemeService.MissionThemeExistsAsync(id))
                {
                    return NotFound(new { message = $"Mission theme with ID {id} not found." });
                }

                // Check if theme name already exists (excluding current theme)
                if (await _missionThemeService.MissionThemeNameExistsAsync(request.Name, id))
                {
                    return BadRequest(new { message = "A mission theme with this name already exists." });
                }

                var updatedTheme = await _missionThemeService.UpdateMissionThemeAsync(id, request, userId.Value);

                if (updatedTheme == null)
                    return NotFound(new { message = $"Mission theme with ID {id} not found." });

                return Ok(updatedTheme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the mission theme.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a mission theme (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMissionTheme(int id)
        {
            try
            {
                var success = await _missionThemeService.DeleteMissionThemeAsync(id);

                if (!success)
                    return NotFound(new { message = $"Mission theme with ID {id} not found." });

                return Ok(new { message = "Mission theme deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the mission theme.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active mission themes
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMissionThemes()
        {
            try
            {
                var themes = await _missionThemeService.GetActiveMissionThemesAsync();
                return Ok(themes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active mission themes.", error = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}