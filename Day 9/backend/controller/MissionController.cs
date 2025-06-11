using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Services;
using Movies.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class MissionController : ControllerBase
    {
        private readonly MissionService _missionService;

        public MissionController(MissionService missionService)
        {
            _missionService = missionService;
        }

        /// <summary>
        /// Upload images for missions
        /// </summary>
        [HttpPost]
        [Route("UploadImage")]
        public async Task<IActionResult> UploadImage([FromForm] List<IFormFile> files)
        {
            try
            {
                var fileList = new List<string>();

                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { message = "No files provided." });
                }

                foreach (IFormFile file in files)
                {
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages", "MissionImages");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var name = Path.GetFileNameWithoutExtension(file.FileName);
                    var extension = Path.GetExtension(file.FileName);

                    var unique = name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;

                    var filePath = Path.Combine(uploadFolder, unique);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fPath = Path.Combine("UploadedImages", "MissionImages", unique);
                    fileList.Add(fPath);
                }

                return Ok(new
                {
                    success = true,
                    message = "Images uploaded successfully.",
                    imagePaths = fileList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading images.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all missions with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMissions(
            [FromQuery] string? title = null,
            [FromQuery] string? organisationName = null,
            [FromQuery] int? countryId = null,
            [FromQuery] int? cityId = null,
            [FromQuery] int? themeId = null,
            [FromQuery] string? missionType = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? startDateTo = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] bool sortDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (missions, totalCount) = await _missionService.GetAllMissionsAsync(
                    title, organisationName, countryId, cityId, themeId, missionType,
                    startDateFrom, startDateTo, isActive, sortBy, sortDescending, page, pageSize);

                var response = new
                {
                    Data = missions,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving missions.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get mission by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMission(int id)
        {
            try
            {
                var mission = await _missionService.GetMissionByIdAsync(id);

                if (mission == null)
                    return NotFound(new { message = $"Mission with ID {id} not found." });

                return Ok(mission);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the mission.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new mission (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateMission([FromBody] CreateMissionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Additional validation before creating mission
                if (request.StartDate >= request.EndDate)
                {
                    return BadRequest(new { message = "Start date must be before end date." });
                }

                if (request.RegistrationDeadLine.HasValue && request.RegistrationDeadLine >= request.StartDate)
                {
                    return BadRequest(new { message = "Registration deadline must be before start date." });
                }

                // Map DTO to Mission entity
                var mission = new Mission
                {
                    MissionTitle = request.MissionTitle?.Trim(),
                    MissionDescription = request.MissionDescription?.Trim(),
                    MissionOrganisationName = request.MissionOrganisationName?.Trim(),
                    MissionOrganisationDetail = request.MissionOrganisationDetail?.Trim(),
                    CountryId = request.CountryId,
                    CityId = request.CityId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MissionType = request.MissionType?.Trim(),
                    TotalSheets = request.TotalSheets,
                    RegistrationDeadLine = request.RegistrationDeadLine,
                    MissionThemeId = request.MissionThemeId,
                    MissionSkillId = request.MissionSkillId,
                    MissionImages = request.MissionImages?.Trim(),
                    MissionDocuments = request.MissionDocuments?.Trim(),
                    MissionAvailability = request.MissionAvailability?.Trim(),
                    MissionVideoUrl = request.MissionVideoUrl?.Trim(),
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var createdMission = await _missionService.CreateMissionAsync(mission, userId.Value);
                return CreatedAtAction(nameof(GetMission), new { id = createdMission.Id }, createdMission);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                // Log the full exception details
                var innerException = ex.InnerException?.Message ?? ex.Message;

                // Check for common database constraint violations
                if (innerException.Contains("FOREIGN KEY"))
                {
                    return BadRequest(new { message = "Invalid foreign key reference. Please check Country, City, or Theme IDs." });
                }
                else if (innerException.Contains("UNIQUE"))
                {
                    return BadRequest(new { message = "A mission with this title already exists." });
                }
                else if (innerException.Contains("CHECK"))
                {
                    return BadRequest(new { message = "Data validation failed. Please check all field values." });
                }

                return StatusCode(500, new
                {
                    message = "Database error occurred while creating the mission.",
                    error = innerException
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the mission.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Create a new mission with image upload (Admin only)
        /// </summary>
        [HttpPost]
        [Route("CreateWithImages")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateMissionWithImages([FromForm] CreateMissionFormRequest missionData, [FromForm] List<IFormFile>? images)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Validate dates
                if (missionData.StartDate >= missionData.EndDate)
                {
                    return BadRequest(new { message = "Start date must be before end date." });
                }
                if (missionData.RegistrationDeadLine.HasValue && missionData.RegistrationDeadLine >= missionData.StartDate)
                {
                    return BadRequest(new { message = "Registration deadline must be before start date." });
                }

                // Map form data to Mission entity
                var mission = new Mission
                {
                    MissionTitle = missionData.MissionTitle?.Trim(),
                    MissionDescription = missionData.MissionDescription?.Trim(),
                    MissionOrganisationName = missionData.MissionOrganisationName?.Trim(),
                    MissionOrganisationDetail = missionData.MissionOrganisationDetail?.Trim(),
                    CountryId = missionData.CountryId,
                    CityId = missionData.CityId,
                    // Convert DateTime to UTC to avoid PostgreSQL timezone issues
                    StartDate = DateTime.SpecifyKind(missionData.StartDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind((DateTime)missionData.EndDate, DateTimeKind.Utc),
                    MissionType = missionData.MissionType?.Trim(),
                    TotalSheets = missionData.TotalSheets,
                    // Handle nullable DateTime for RegistrationDeadLine
                    RegistrationDeadLine = missionData.RegistrationDeadLine.HasValue
                        ? DateTime.SpecifyKind(missionData.RegistrationDeadLine.Value, DateTimeKind.Utc)
                        : null,
                    MissionThemeId = missionData.MissionThemeId,
                    MissionSkillId = missionData.MissionSkillId,
                    MissionDocuments = missionData.MissionDocuments?.Trim(),
                    MissionAvailability = missionData.MissionAvailability?.Trim(),
                    MissionVideoUrl = missionData.MissionVideoUrl?.Trim(),
                    IsActive = missionData.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                // Upload images if provided
                if (images != null && images.Count > 0)
                {
                    var imageResult = await UploadImagesInternal(images);
                    if (imageResult.Count > 0)
                    {
                        mission.MissionImages = string.Join(",", imageResult);
                    }
                }

                var createdMission = await _missionService.CreateMissionAsync(mission, userId.Value);
                return CreatedAtAction(
                    nameof(GetMission),
                    new { id = createdMission.Id },
                    createdMission);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the mission.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing mission (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateMission(int id, [FromBody] UpdateMissionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if mission exists
                var existingMission = await _missionService.GetMissionByIdAsync(id);
                if (existingMission == null)
                {
                    return NotFound(new { message = $"Mission with ID {id} not found." });
                }

                // Map DTO to Mission entity
                var missionToUpdate = new Mission
                {
                    MissionTitle = request.MissionTitle,
                    MissionDescription = request.MissionDescription,
                    MissionOrganisationName = request.MissionOrganisationName,
                    MissionOrganisationDetail = request.MissionOrganisationDetail,
                    CountryId = request.CountryId,
                    CityId = request.CityId,
                    MissionThemeId = request.MissionThemeId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MissionType = request.MissionType,
                    TotalSheets = request.TotalSheets,
                    RegistrationDeadLine = request.RegistrationDeadLine,
                    MissionSkillId = request.MissionSkillId,
                    MissionImages = request.MissionImages,
                    MissionDocuments = request.MissionDocuments,
                    MissionAvailability = request.MissionAvailability,
                    MissionVideoUrl = request.MissionVideoUrl,
                    IsActive = request.IsActive
                };

                var updatedMission = await _missionService.UpdateMissionAsync(id, missionToUpdate, userId.Value);
                if (updatedMission == null)
                    return NotFound(new { message = $"Mission with ID {id} not found." });

                return Ok(updatedMission);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the mission.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a mission (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMission(int id)
        {
            try
            {
                var success = await _missionService.DeleteMissionAsync(id);

                if (!success)
                    return NotFound(new { message = $"Mission with ID {id} not found." });

                return Ok(new { message = "Mission deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the mission.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active missions
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMissions()
        {
            try
            {
                var missions = await _missionService.GetActiveMissionsAsync();
                return Ok(missions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active missions.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get upcoming missions
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingMissions()
        {
            try
            {
                var missions = await _missionService.GetUpcomingMissionsAsync();
                return Ok(missions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming missions.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get missions by theme
        /// </summary>
        [HttpGet("theme/{themeId}")]
        public async Task<IActionResult> GetMissionsByTheme(int themeId)
        {
            try
            {
                var missions = await _missionService.GetMissionsByThemeAsync(themeId);
                return Ok(missions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving missions by theme.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get missions by location
        /// </summary>
        [HttpGet("location/{countryId}")]
        public async Task<IActionResult> GetMissionsByLocation(int countryId, [FromQuery] int? cityId = null)
        {
            try
            {
                var missions = await _missionService.GetMissionsByLocationAsync(countryId, cityId);
                return Ok(missions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving missions by location.", error = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private async Task<List<string>> UploadImagesInternal(List<IFormFile> files)
        {
            var fileList = new List<string>();
            foreach (IFormFile file in files)
            {
                // Changed to your specified path
                var uploadFolder = @"C:\Users\HP\OneDrive\Desktop\sm\project\public";

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var name = Path.GetFileNameWithoutExtension(file.FileName);
                var extension = Path.GetExtension(file.FileName);
                var unique = name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                var filePath = Path.Combine(uploadFolder, unique);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return just the filename since it's now in the public folder
                fileList.Add(unique);
            }
            return fileList;
        }
    }

    // Form request model for CreateWithImages endpoint
    public class CreateMissionFormRequest
    {
        [Required]
        [MaxLength(200)]
        public string MissionTitle { get; set; }

        [Required]
        [MaxLength(2000)]
        public string MissionDescription { get; set; }

        [Required]
        [MaxLength(200)]
        public string MissionOrganisationName { get; set; }

        [MaxLength(1000)]
        public string? MissionOrganisationDetail { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int CityId { get; set; }

        [Required]
        public int MissionThemeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string MissionType { get; set; }

        public int? TotalSheets { get; set; }
        public DateTime? RegistrationDeadLine { get; set; }
        public int? MissionSkillId { get; set; }

        [MaxLength(2000)]
        public string? MissionDocuments { get; set; }

        [MaxLength(50)]
        public string? MissionAvailability { get; set; }

        [MaxLength(500)]
        public string? MissionVideoUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}