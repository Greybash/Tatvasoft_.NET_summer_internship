using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;
using System.ComponentModel.DataAnnotations;

namespace Movies.Services
{
    public class UserDetailService
    {
        private readonly MoviesDbContext _context;

        public UserDetailService(MoviesDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get user detail by user ID with related data (Country, City)
        /// </summary>
        public async Task<UserDetail?> GetUserDetailByUserIdAsync(int userId)
        {
            return await _context.UserDetails
                .Include(ud => ud.User)
                .Include(ud => ud.Country)
                .Include(ud => ud.City)
                .FirstOrDefaultAsync(ud => ud.UserId == userId);
        }

        /// <summary>
        /// Get user detail by ID with related data
        /// </summary>
        public async Task<UserDetail?> GetUserDetailByIdAsync(int id)
        {
            return await _context.UserDetails
                .Include(ud => ud.User)
                .Include(ud => ud.Country)
                .Include(ud => ud.City)
                .FirstOrDefaultAsync(ud => ud.Id == id);
        }

        /// <summary>
        /// Create a new user detail profile
        /// </summary>
        public async Task<UserDetail> CreateUserDetailAsync(UserDetail userDetail)
        {
            // Validate that user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userDetail.UserId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userDetail.UserId} does not exist.");
            }

            // Check if user detail already exists for this user
            var existingDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.UserId == userDetail.UserId);
            if (existingDetail != null)
            {
                throw new ArgumentException($"User detail already exists for User ID {userDetail.UserId}.");
            }

            // Validate foreign key references
            await ValidateForeignKeyReferencesAsync(userDetail);

            _context.UserDetails.Add(userDetail);
            await _context.SaveChangesAsync();

            // Return the created user detail with related data
            return await GetUserDetailByIdAsync(userDetail.Id) ?? userDetail;
        }

        /// <summary>
        /// Update an existing user detail
        /// </summary>
        public async Task<UserDetail?> UpdateUserDetailAsync(int userId, UserDetail updatedUserDetail)
        {
            var existingUserDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.UserId == userId);

            if (existingUserDetail == null)
                return null;

            // Validate foreign key references before updating
            var validationErrors = new List<string>();

            // Validate Country
            if (updatedUserDetail.CountryId > 0)
            {
                var countryExists = await _context.Countries.AnyAsync(c => c.Id == updatedUserDetail.CountryId);
                if (!countryExists)
                {
                    validationErrors.Add($"Country with ID {updatedUserDetail.CountryId} does not exist.");
                }
            }

            // Validate City
            if (updatedUserDetail.CityId > 0)
            {
                var cityExists = await _context.Cities.AnyAsync(c => c.Id == updatedUserDetail.CityId);
                if (!cityExists)
                {
                    validationErrors.Add($"City with ID {updatedUserDetail.CityId} does not exist.");
                }

                // Also validate that city belongs to the selected country
                if (updatedUserDetail.CountryId > 0)
                {
                    var cityBelongsToCountry = await _context.Cities
                        .AnyAsync(c => c.Id == updatedUserDetail.CityId && c.CountryId == updatedUserDetail.CountryId);
                    if (!cityBelongsToCountry)
                    {
                        validationErrors.Add("The selected city does not belong to the selected country.");
                    }
                }
            }

            // If there are validation errors, throw an exception with details
            if (validationErrors.Any())
            {
                throw new ValidationException(string.Join("; ", validationErrors));
            }

            // Update properties
            existingUserDetail.Name = updatedUserDetail.Name;
            existingUserDetail.Surname = updatedUserDetail.Surname;
            existingUserDetail.EmployeeId = updatedUserDetail.EmployeeId;
            existingUserDetail.Manager = updatedUserDetail.Manager;
            existingUserDetail.Title = updatedUserDetail.Title;
            existingUserDetail.Department = updatedUserDetail.Department;
            existingUserDetail.MyProfile = updatedUserDetail.MyProfile;
            existingUserDetail.WhyIVolunteer = updatedUserDetail.WhyIVolunteer;
            existingUserDetail.CountryId = updatedUserDetail.CountryId;
            existingUserDetail.CityId = updatedUserDetail.CityId;
            existingUserDetail.Availability = updatedUserDetail.Availability;
            existingUserDetail.LinkedInUrl = updatedUserDetail.LinkedInUrl;
            existingUserDetail.MySkills = updatedUserDetail.MySkills;
            existingUserDetail.Status = updatedUserDetail.Status;

            // Only update UserImage if a new one is provided
            if (!string.IsNullOrEmpty(updatedUserDetail.UserImage))
            {
                existingUserDetail.UserImage = updatedUserDetail.UserImage;
            }

            await _context.SaveChangesAsync();

            // Return updated user detail with related data
            return await GetUserDetailByUserIdAsync(userId);
        }

        /// <summary>
        /// Delete a user detail
        /// </summary>
        public async Task<bool> DeleteUserDetailAsync(int userId)
        {
            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.UserId == userId);

            if (userDetail == null)
                return false;

            _context.UserDetails.Remove(userDetail);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Check if user detail exists for a user
        /// </summary>
        public async Task<bool> UserDetailExistsAsync(int userId)
        {
            return await _context.UserDetails.AnyAsync(ud => ud.UserId == userId);
        }

        /// <summary>
        /// Validate foreign key references exist
        /// </summary>
        private async Task ValidateForeignKeyReferencesAsync(UserDetail userDetail)
        {
            // Check if Country exists
            if (!await _context.Countries.AnyAsync(c => c.Id == userDetail.CountryId))
            {
                throw new ArgumentException($"Country with ID {userDetail.CountryId} does not exist.");
            }

            // Check if City exists
            if (!await _context.Cities.AnyAsync(c => c.Id == userDetail.CityId))
            {
                throw new ArgumentException($"City with ID {userDetail.CityId} does not exist.");
            }

            // Validate that City belongs to the specified Country
            var city = await _context.Cities.FindAsync(userDetail.CityId);
            if (city != null && city.CountryId != userDetail.CountryId)
            {
                throw new ArgumentException($"City with ID {userDetail.CityId} does not belong to Country with ID {userDetail.CountryId}.");
            }
        }


    }
}