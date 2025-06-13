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
    public class MissionSkillController : ControllerBase
    {
        private readonly MissionSkillService _missionSkillService;

        public MissionSkillController(MissionSkillService missionSkillService)
        {
            _missionSkillService = missionSkillService;
        }

        /// <summary>
        /// Get all mission skills with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMissionSkills(
            [FromQuery] string? name = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (skills, totalCount) = await _missionSkillService.GetAllMissionSkillsAsync(
                    name, isActive, sortBy, sortDescending, page, pageSize);

                var response = new
                {
                    Data = skills,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving mission skills.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get mission skill by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMissionSkill(int id)
        {
            try
            {
                var skill = await _missionSkillService.GetMissionSkillByIdAsync(id);

                if (skill == null)
                    return NotFound(new { message = $"Mission skill with ID {id} not found." });

                return Ok(skill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the mission skill.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new mission skill (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateMissionSkill([FromBody] MissionSkill request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if skill name already exists
                if (await _missionSkillService.MissionSkillNameExistsAsync(request.Name))
                {
                    return BadRequest(new { message = "A mission skill with this name already exists." });
                }

                var createdSkill = await _missionSkillService.CreateMissionSkillAsync(request, userId.Value);

                return CreatedAtAction(
                    nameof(GetMissionSkill),
                    new { id = createdSkill.Id },
                    createdSkill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the mission skill.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing mission skill (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateMissionSkill(int id, [FromBody] MissionSkill request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if skill exists
                if (!await _missionSkillService.MissionSkillExistsAsync(id))
                {
                    return NotFound(new { message = $"Mission skill with ID {id} not found." });
                }

                // Check if skill name already exists (excluding current skill)
                if (await _missionSkillService.MissionSkillNameExistsAsync(request.Name, id))
                {
                    return BadRequest(new { message = "A mission skill with this name already exists." });
                }

                var updatedSkill = await _missionSkillService.UpdateMissionSkillAsync(id, request, userId.Value);

                if (updatedSkill == null)
                    return NotFound(new { message = $"Mission skill with ID {id} not found." });

                return Ok(updatedSkill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the mission skill.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a mission skill (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMissionSkill(int id)
        {
            try
            {
                var success = await _missionSkillService.DeleteMissionSkillAsync(id);

                if (!success)
                    return NotFound(new { message = $"Mission skill with ID {id} not found." });

                return Ok(new { message = "Mission skill deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the mission skill.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active mission skills
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMissionSkills()
        {
            try
            {
                var skills = await _missionSkillService.GetActiveMissionSkillsAsync();
                return Ok(skills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active mission skills.", error = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}