using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Services;
using Movies.Models;
using System.Security.Claims;
using Movies.Dto;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MissionApplicationController : ControllerBase
    {
        private readonly MissionApplicationService _missionApplicationService;

        public MissionApplicationController(MissionApplicationService missionApplicationService)
        {
            _missionApplicationService = missionApplicationService;
        }

        // User endpoint - Apply for a mission
        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> Apply([FromQuery] int userId, [FromBody] Movies.Services.MissionApplicationRequest request)
        {
            var result = await _missionApplicationService.ApplyForMissionAsync(userId, request);
            if (!result.Success)
                return BadRequest(new { result.ErrorMessage });

            return Ok(new { result.ApplicationId });
        }
        // User endpoint - Get user's own applications
        [HttpGet("my-applications")]
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> GetMyApplications()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid user token");
                }

                var applications = await _missionApplicationService.GetUserApplicationsAsync(userId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching applications", error = ex.Message });
            }
        }

        // User endpoint - Cancel application (only if pending)
        [HttpDelete("cancel/{applicationId}")]
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> CancelApplication(int applicationId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid user token");
                }

                var result = await _missionApplicationService.CancelApplicationAsync(applicationId, userId);

                if (result.Success)
                {
                    return Ok(new { message = "Application cancelled successfully" });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while cancelling application", error = ex.Message });
            }
        }

        // Admin endpoint - Get all pending applications
        [HttpGet("admin/pending")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPendingApplications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var applications = await _missionApplicationService.GetPendingApplicationsAsync(page, pageSize);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching pending applications", error = ex.Message });
            }
        }

        // Admin endpoint - Get all applications with filters
        [HttpGet("admin/all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllApplications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] int? missionId = null)
        {
            try
            {
                var applications = await _missionApplicationService.GetAllApplicationsAsync(page, pageSize, status, missionId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching applications", error = ex.Message });
            }
        }

        // Admin endpoint - Approve application
        [HttpPut("admin/approve/{applicationId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApproveApplication(int applicationId, [FromBody] AdminActionRequest request)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
                {
                    return Unauthorized("Invalid admin token");
                }

                var result = await _missionApplicationService.ApproveApplicationAsync(applicationId, adminId, request.Comments);

                if (result.Success)
                {
                    return Ok(new { message = "Application approved successfully" });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while approving application", error = ex.Message });
            }
        }

        // Admin endpoint - Reject application
        [HttpPut("admin/reject/{applicationId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RejectApplication(int applicationId, [FromBody] AdminActionRequest request)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
                {
                    return Unauthorized("Invalid admin token");
                }

                var result = await _missionApplicationService.RejectApplicationAsync(applicationId, adminId, request.Comments);

                if (result.Success)
                {
                    return Ok(new { message = "Application rejected successfully" });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while rejecting application", error = ex.Message });
            }
        }

        // Admin endpoint - Get application statistics
        [HttpGet("admin/statistics")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetApplicationStatistics()
        {
            try
            {
                var stats = await _missionApplicationService.GetApplicationStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching statistics", error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class MissionApplicationRequest
    {
        public int MissionId { get; set; }
        public int Seats { get; set; } = 1;
        public string? Message { get; set; }
    }

    public class AdminActionRequest
    {
        public string? Comments { get; set; }
    }
}