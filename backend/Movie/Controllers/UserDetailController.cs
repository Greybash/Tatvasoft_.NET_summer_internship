using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Services;
using Movies.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class UserDetailController : ControllerBase
    {
        private readonly UserDetailService _userDetailService;

        public UserDetailController(UserDetailService userDetailService)
        {
            _userDetailService = userDetailService;
        }

        /// <summary>
        /// Get current user's profile detail
        /// </summary>
        [HttpGet]
        [Route("MyProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                var userDetail = await _userDetailService.GetUserDetailByUserIdAsync(userId.Value);

                if (userDetail == null)
                    return NotFound(new { message = "User profile not found." });

                return Ok(userDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user detail by user ID (for viewing other user's profile)
        /// </summary>
        [HttpGet]
        [Route("User/{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            try
            {
                var userDetail = await _userDetailService.GetUserDetailByUserIdAsync(userId);

                if (userDetail == null)
                    return NotFound(new { message = $"User profile for User ID {userId} not found." });

                return Ok(userDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create user profile detail
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUserDetail([FromBody] CreateUserDetailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if user detail already exists
                var existingDetail = await _userDetailService.UserDetailExistsAsync(userId.Value);
                if (existingDetail)
                {
                    return BadRequest(new { message = "User profile already exists. Use update endpoint instead." });
                }

                // Map DTO to UserDetail entity
                var userDetail = new UserDetail
                {
                    UserId = userId.Value,
                    Name = request.Name?.Trim() ?? string.Empty,
                    Surname = request.Surname?.Trim() ?? string.Empty,
                    EmployeeId = request.EmployeeId?.Trim() ?? string.Empty,
                    Manager = request.Manager?.Trim() ?? string.Empty,
                    Title = request.Title?.Trim() ?? string.Empty,
                    Department = request.Department?.Trim() ?? string.Empty,
                    MyProfile = request.MyProfile?.Trim() ?? string.Empty,
                    WhyIVolunteer = request.WhyIVolunteer?.Trim() ?? string.Empty,
                    CountryId = request.CountryId,
                    CityId = request.CityId,
                    Availability = request.Availability?.Trim() ?? string.Empty,
                    LinkedInUrl = request.LinkedInUrl?.Trim() ?? string.Empty,
                    MySkills = request.MySkills?.Trim() ?? string.Empty,
                    UserImage = string.Empty,
                    Status = request.Status
                };

                var createdUserDetail = await _userDetailService.CreateUserDetailAsync(userDetail);
                return Ok(new
                {
                    success = true,
                    message = "User profile created successfully.",
                    data = createdUserDetail
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;

                if (innerException.Contains("FOREIGN KEY"))
                {
                    return BadRequest(new { message = "Invalid foreign key reference. Please check Country or City IDs." });
                }
                else if (innerException.Contains("UNIQUE"))
                {
                    return BadRequest(new { message = "A user profile with this information already exists." });
                }

                return StatusCode(500, new
                {
                    message = "Database error occurred while creating user profile.",
                    error = innerException
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating user profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update current user's profile detail
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserDetailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if user detail exists
                var existingUserDetail = await _userDetailService.GetUserDetailByUserIdAsync(userId.Value);
                if (existingUserDetail == null)
                {
                    return NotFound(new { message = "User profile not found. Create profile first." });
                }

                // Map DTO to UserDetail entity
                var userDetailToUpdate = new UserDetail
                {
                    Name = request.Name?.Trim() ?? string.Empty,
                    Surname = request.Surname?.Trim() ?? string.Empty,
                    EmployeeId = request.EmployeeId?.Trim() ?? string.Empty,
                    Manager = request.Manager?.Trim() ?? string.Empty,
                    Title = request.Title?.Trim() ?? string.Empty,
                    Department = request.Department?.Trim() ?? string.Empty,
                    MyProfile = request.MyProfile?.Trim() ?? string.Empty,
                    WhyIVolunteer = request.WhyIVolunteer?.Trim() ?? string.Empty,
                    CountryId = request.CountryId,
                    CityId = request.CityId,
                    Availability = request.Availability?.Trim() ?? string.Empty,
                    LinkedInUrl = request.LinkedInUrl?.Trim() ?? string.Empty,
                    MySkills = request.MySkills?.Trim() ?? string.Empty,
                    Status = request.Status,
                    UserImage = string.Empty // Will be preserved in service if not updated
                };

                var updatedUserDetail = await _userDetailService.UpdateUserDetailAsync(userId.Value, userDetailToUpdate);
                if (updatedUserDetail == null)
                    return NotFound(new { message = "User profile not found." });

                return Ok(new
                {
                    success = true,
                    message = "User profile updated successfully.",
                    data = updatedUserDetail
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;

                if (innerException.Contains("FOREIGN KEY"))
                {
                    return BadRequest(new { message = "Invalid foreign key reference. Please check Country or City IDs." });
                }

                return StatusCode(500, new
                {
                    message = "Database error occurred while updating user profile.",
                    error = innerException
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete current user's profile detail
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteMyProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                var result = await _userDetailService.DeleteUserDetailAsync(userId.Value);
                if (!result)
                    return NotFound(new { message = "User profile not found." });

                return Ok(new
                {
                    success = true,
                    message = "User profile deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting user profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Upload profile image for user
        /// </summary>
        [HttpPost]
        [Route("UploadProfileImage")]
        public async Task<IActionResult> UploadProfileImage([FromForm] List<IFormFile> files)
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
                return StatusCode(500, new { message = "An error occurred while uploading the profile image.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create user profile with image upload
        /// </summary>
        [HttpPost]
        [Route("CreateWithImage")]
        public async Task<IActionResult> CreateUserDetailWithImage([FromForm] CreateUserDetailFormRequest userDetailData, [FromForm] List<IFormFile>? profileImages)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if user detail already exists
                var existingDetail = await _userDetailService.UserDetailExistsAsync(userId.Value);
                if (existingDetail)
                {
                    return BadRequest(new { message = "User profile already exists. Use update endpoint instead." });
                }

                // Map form data to UserDetail entity
                var userDetail = new UserDetail
                {
                    UserId = userId.Value,
                    Name = userDetailData.Name?.Trim() ?? string.Empty,
                    Surname = userDetailData.Surname?.Trim() ?? string.Empty,
                    EmployeeId = userDetailData.EmployeeId?.Trim() ?? string.Empty,
                    Manager = userDetailData.Manager?.Trim() ?? string.Empty,
                    Title = userDetailData.Title?.Trim() ?? string.Empty,
                    Department = userDetailData.Department?.Trim() ?? string.Empty,
                    MyProfile = userDetailData.MyProfile?.Trim() ?? string.Empty,
                    WhyIVolunteer = userDetailData.WhyIVolunteer?.Trim() ?? string.Empty,
                    CountryId = userDetailData.CountryId,
                    CityId = userDetailData.CityId,
                    Availability = userDetailData.Availability?.Trim() ?? string.Empty,
                    LinkedInUrl = userDetailData.LinkedInUrl?.Trim() ?? string.Empty,
                    MySkills = userDetailData.MySkills?.Trim() ?? string.Empty,
                    UserImage = string.Empty,
                    Status = userDetailData.Status
                };

                // Upload profile image if provided
                if (profileImages != null && profileImages.Count > 0)
                {
                    var imageResult = await UploadProfileImageInternal(profileImages);
                    if (imageResult.Count > 0)
                    {
                        userDetail.UserImage = string.Join(",", imageResult);
                    }
                }

                var createdUserDetail = await _userDetailService.CreateUserDetailAsync(userDetail);
                return Ok(new
                {
                    success = true,
                    message = "User profile created successfully.",
                    data = createdUserDetail
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating user profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update current user's profile detail with image upload
        /// </summary>
        [HttpPut]
        [Route("UpdateWithImage")]
        public async Task<IActionResult> UpdateMyProfileWithImage([FromForm] UpdateUserDetailFormRequest userDetailData, [FromForm] List<IFormFile>? profileImages)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User ID not found in token." });

                // Check if user detail exists
                var existingUserDetail = await _userDetailService.GetUserDetailByUserIdAsync(userId.Value);
                if (existingUserDetail == null)
                {
                    return NotFound(new { message = "User profile not found. Create profile first." });
                }

                // Map form data to UserDetail entity
                var userDetailToUpdate = new UserDetail
                {
                    Name = userDetailData.Name?.Trim() ?? string.Empty,
                    Surname = userDetailData.Surname?.Trim() ?? string.Empty,
                    EmployeeId = userDetailData.EmployeeId?.Trim() ?? string.Empty,
                    Manager = userDetailData.Manager?.Trim() ?? string.Empty,
                    Title = userDetailData.Title?.Trim() ?? string.Empty,
                    Department = userDetailData.Department?.Trim() ?? string.Empty,
                    MyProfile = userDetailData.MyProfile?.Trim() ?? string.Empty,
                    WhyIVolunteer = userDetailData.WhyIVolunteer?.Trim() ?? string.Empty,
                    CountryId = userDetailData.CountryId,
                    CityId = userDetailData.CityId,
                    Availability = userDetailData.Availability?.Trim() ?? string.Empty,
                    LinkedInUrl = userDetailData.LinkedInUrl?.Trim() ?? string.Empty,
                    MySkills = userDetailData.MySkills?.Trim() ?? string.Empty,
                    Status = userDetailData.Status,
                    UserImage = existingUserDetail.UserImage // Preserve existing image if no new one uploaded
                };

                // Upload profile image if provided
                if (profileImages != null && profileImages.Count > 0 )
                {
                    var imageResult = await UploadProfileImageInternal(profileImages);
                    if (imageResult.Count > 0)
                    {
                        userDetailToUpdate.UserImage =  string.Join(",", imageResult);
                    }
                }

                var updatedUserDetail = await _userDetailService.UpdateUserDetailAsync(userId.Value, userDetailToUpdate);
                if (updatedUserDetail == null)
                    return NotFound(new { message = "User profile not found." });

                return Ok(new
                {
                    success = true,
                    message = "User profile updated successfully.",
                    data = updatedUserDetail
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user profile.", error = ex.Message });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT token claims
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        /// <summary>
        /// Internal helper method for uploading profile images
        /// </summary>
        private async Task<List<string>> UploadProfileImageInternal(List<IFormFile> files)
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

    #endregion
}
