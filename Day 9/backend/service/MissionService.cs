using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Movies.Services
{
    public class MissionService
    {
        private readonly MoviesDbContext _context;

        public MissionService(MoviesDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all missions with filtering, sorting, and pagination
        /// </summary>
        public async Task<(List<Mission> missions, int totalCount)> GetAllMissionsAsync(
            string? title = null,
            string? organisationName = null,
            int? countryId = null,
            int? cityId = null,
            int? themeId = null,
            string? missionType = null,
            DateTime? startDateFrom = null,
            DateTime? startDateTo = null,
            bool? isActive = null,
            string? sortBy = "createdAt",
            bool sortDescending = true,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.MissionTitle.Contains(title));
            }

            if (!string.IsNullOrWhiteSpace(organisationName))
            {
                query = query.Where(m => m.MissionOrganisationName.Contains(organisationName));
            }

            if (countryId.HasValue)
            {
                query = query.Where(m => m.CountryId == countryId.Value);
            }

            if (cityId.HasValue)
            {
                query = query.Where(m => m.CityId == cityId.Value);
            }

            if (themeId.HasValue)
            {
                query = query.Where(m => m.MissionThemeId == themeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(missionType))
            {
                query = query.Where(m => m.MissionType == missionType);
            }

            if (startDateFrom.HasValue)
            {
                query = query.Where(m => m.StartDate >= startDateFrom.Value);
            }

            if (startDateTo.HasValue)
            {
                query = query.Where(m => m.StartDate <= startDateTo.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, sortBy, sortDescending);

            // Apply pagination
            var missions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (missions, totalCount);
        }

        /// <summary>
        /// Get mission by ID with related data
        /// </summary>
        public async Task<Mission?> GetMissionByIdAsync(int id)
        {
            return await _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Create a new mission
        /// </summary>
        public async Task<Mission> CreateMissionAsync(Mission mission, int userId)
        {
            // Validate foreign key references
            await ValidateForeignKeyReferencesAsync(mission);

            // Set audit fields
            mission.CreatedAt = DateTime.UtcNow;
            mission.UpdatedAt = null;

            _context.Missions.Add(mission);
            await _context.SaveChangesAsync();

            // Return the created mission with related data
            return await GetMissionByIdAsync(mission.Id) ?? mission;
        }

        /// <summary>
        /// Update an existing mission
        /// </summary>
        public async Task<Mission?> UpdateMissionAsync(int id, Mission updatedMission, int userId)
        {
            var existingMission = await _context.Missions.FindAsync(id);
            if (existingMission == null)
                return null;

            // Validate foreign key references before updating
            var validationErrors = new List<string>();

            // Validate Country
            if (updatedMission.CountryId > 0)
            {
                var countryExists = await _context.Countries.AnyAsync(c => c.Id == updatedMission.CountryId);
                if (!countryExists)
                {
                    validationErrors.Add($"Country with ID {updatedMission.CountryId} does not exist.");
                }
            }

            // Validate City
            if (updatedMission.CityId > 0)
            {
                var cityExists = await _context.Cities.AnyAsync(c => c.Id == updatedMission.CityId);
                if (!cityExists)
                {
                    validationErrors.Add($"City with ID {updatedMission.CityId} does not exist.");
                }

                // Also validate that city belongs to the selected country
                if (updatedMission.CountryId > 0)
                {
                    var cityBelongsToCountry = await _context.Cities
                        .AnyAsync(c => c.Id == updatedMission.CityId && c.CountryId == updatedMission.CountryId);
                    if (!cityBelongsToCountry)
                    {
                        validationErrors.Add("The selected city does not belong to the selected country.");
                    }
                }
            }

            // Validate MissionTheme
            if (updatedMission.MissionThemeId > 0)
            {
                var themeExists = await _context.MissionThemes.AnyAsync(t => t.Id == updatedMission.MissionThemeId);
                if (!themeExists)
                {
                    validationErrors.Add($"Mission Theme with ID {updatedMission.MissionThemeId} does not exist.");
                }
            }

            // Validate MissionSkill (if provided) - FIXED: Removed duplicate validation
            if (updatedMission.MissionSkillId.HasValue && updatedMission.MissionSkillId.Value > 0)
            {
                var skillExists = await _context.MissionSkills.AnyAsync(s => s.Id == updatedMission.MissionSkillId.Value);
                if (!skillExists)
                {
                    validationErrors.Add($"Mission Skill with ID {updatedMission.MissionSkillId} does not exist.");
                }
            }

            // If there are validation errors, throw an exception with details
            if (validationErrors.Any())
            {
                throw new ValidationException(string.Join("; ", validationErrors));
            }

            // Update scalar properties only — avoid updating navigation properties directly from the request
            existingMission.MissionTitle = updatedMission.MissionTitle;
            existingMission.MissionDescription = updatedMission.MissionDescription;
            existingMission.MissionOrganisationName = updatedMission.MissionOrganisationName;
            existingMission.MissionOrganisationDetail = updatedMission.MissionOrganisationDetail;
            existingMission.CountryId = updatedMission.CountryId;
            existingMission.CityId = updatedMission.CityId;
            existingMission.MissionThemeId = updatedMission.MissionThemeId;
            existingMission.StartDate = updatedMission.StartDate;
            existingMission.EndDate = updatedMission.EndDate;
            existingMission.MissionType = updatedMission.MissionType;
            existingMission.TotalSheets = updatedMission.TotalSheets;
            existingMission.RegistrationDeadLine = updatedMission.RegistrationDeadLine;
            existingMission.MissionSkillId = updatedMission.MissionSkillId;

            existingMission.MissionDocuments = updatedMission.MissionDocuments;
            existingMission.MissionAvailability = updatedMission.MissionAvailability;
            existingMission.MissionVideoUrl = updatedMission.MissionVideoUrl;
            existingMission.IsActive = updatedMission.IsActive;
            existingMission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return updated mission with related data if needed
            return await GetMissionByIdAsync(id);
        }


        /// <summary>
        /// Delete a mission
        /// </summary>
        public async Task<bool> DeleteMissionAsync(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission == null)
                return false;

            _context.Missions.Remove(mission);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Check if mission exists
        /// </summary>
        public async Task<bool> MissionExistsAsync(int id)
        {
            return await _context.Missions.AnyAsync(m => m.Id == id);
        }

        /// <summary>
        /// Get all active missions
        /// </summary>
        public async Task<List<Mission>> GetActiveMissionsAsync()
        {
            return await _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get upcoming missions (missions that haven't started yet)
        /// </summary>
        public async Task<List<Mission>> GetUpcomingMissionsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .Where(m => m.IsActive && m.StartDate > currentDate)
                .OrderBy(m => m.StartDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get missions by theme
        /// </summary>
        public async Task<List<Mission>> GetMissionsByThemeAsync(int themeId)
        {
            return await _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .Where(m => m.MissionThemeId == themeId && m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get missions by location (country and optionally city)
        /// </summary>
        public async Task<List<Mission>> GetMissionsByLocationAsync(int countryId, int? cityId = null)
        {
            var query = _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .Where(m => m.CountryId == countryId && m.IsActive);

            if (cityId.HasValue)
            {
                query = query.Where(m => m.CityId == cityId.Value);
            }

            return await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get ongoing missions (missions that have started but not ended)
        /// </summary>
        public async Task<List<Mission>> GetOngoingMissionsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .Where(m => m.IsActive &&
                           m.StartDate <= currentDate &&
                           (m.EndDate == null || m.EndDate > currentDate))
                .OrderBy(m => m.StartDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get missions by organization
        /// </summary>
        public async Task<List<Mission>> GetMissionsByOrganizationAsync(string organizationName)
        {
            return await _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .Where(m => m.MissionOrganisationName.Contains(organizationName) && m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Search missions by keywords in title and description
        /// </summary>
        public async Task<List<Mission>> SearchMissionsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Mission>();

            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .Where(m => m.IsActive &&
                           (m.MissionTitle.ToLower().Contains(lowerSearchTerm) ||
                            m.MissionDescription.ToLower().Contains(lowerSearchTerm) ||
                            m.MissionOrganisationName.ToLower().Contains(lowerSearchTerm)))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get missions with registration deadline approaching (within specified days)
        /// </summary>
        public async Task<List<Mission>> GetMissionsWithApproachingDeadlineAsync(int daysFromNow = 7)
        {
            var currentDate = DateTime.UtcNow;
            var deadlineDate = currentDate.AddDays(daysFromNow);

            return await _context.Missions
                .Include(m => m.Country)
                .Include(m => m.City)
                .Include(m => m.MissionTheme)
                .Where(m => m.IsActive &&
                           m.RegistrationDeadLine.HasValue &&
                           m.RegistrationDeadLine >= currentDate &&
                           m.RegistrationDeadLine <= deadlineDate)
                .OrderBy(m => m.RegistrationDeadLine)
                .ToListAsync();
        }

        /// <summary>
        /// Get mission statistics
        /// </summary>
        public async Task<MissionStatistics> GetMissionStatisticsAsync()
        {
            var currentDate = DateTime.UtcNow;

            var totalMissions = await _context.Missions.CountAsync();
            var activeMissions = await _context.Missions.CountAsync(m => m.IsActive);
            var upcomingMissions = await _context.Missions.CountAsync(m => m.IsActive && m.StartDate > currentDate);
            var ongoingMissions = await _context.Missions.CountAsync(m => m.IsActive &&
                                                                        m.StartDate <= currentDate &&
                                                                        (m.EndDate == null || m.EndDate > currentDate));
            var completedMissions = await _context.Missions.CountAsync(m => m.EndDate.HasValue && m.EndDate < currentDate);

            return new MissionStatistics
            {
                TotalMissions = totalMissions,
                ActiveMissions = activeMissions,
                UpcomingMissions = upcomingMissions,
                OngoingMissions = ongoingMissions,
                CompletedMissions = completedMissions
            };
        }

        #region Private Methods

        /// <summary>
        /// Apply sorting to the query
        /// </summary>
        private IQueryable<Mission> ApplySorting(IQueryable<Mission> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "createdAt";

            Expression<Func<Mission, object>> sortExpression = sortBy.ToLower() switch
            {
                "title" => m => m.MissionTitle,
                "organisation" or "organization" => m => m.MissionOrganisationName,
                "startdate" => m => m.StartDate,
                "enddate" => m => m.EndDate ?? DateTime.MaxValue,
                "missiontype" => m => m.MissionType,
                "country" => m => m.Country.Name,
                "city" => m => m.City.Name,
                "theme" => m => m.MissionTheme.Name,
                "updatedat" => m => m.UpdatedAt ?? DateTime.MinValue,
                _ => m => m.CreatedAt
            };

            return sortDescending
                ? query.OrderByDescending(sortExpression)
                : query.OrderBy(sortExpression);
        }

        /// <summary>
        /// Validate foreign key references exist
        /// </summary>
        private async Task ValidateForeignKeyReferencesAsync(Mission mission)
        {
            // Check if Country exists
            if (!await _context.Countries.AnyAsync(c => c.Id == mission.CountryId))
            {
                throw new ArgumentException($"Country with ID {mission.CountryId} does not exist.");
            }

            // Check if City exists
            if (!await _context.Cities.AnyAsync(c => c.Id == mission.CityId))
            {
                throw new ArgumentException($"City with ID {mission.CityId} does not exist.");
            }

            // Check if MissionTheme exists
            if (!await _context.MissionThemes.AnyAsync(mt => mt.Id == mission.MissionThemeId))
            {
                throw new ArgumentException($"Mission Theme with ID {mission.MissionThemeId} does not exist.");
            }

            // Check if MissionSkill exists (if provided)
            if (mission.MissionSkillId.HasValue &&
                !await _context.MissionSkills.AnyAsync(ms => ms.Id == mission.MissionSkillId.Value))
            {
                throw new ArgumentException($"Mission Skill with ID {mission.MissionSkillId} does not exist.");
            }

            // Optionally validate that City belongs to the specified Country
            var city = await _context.Cities.FindAsync(mission.CityId);
            if (city != null && city.CountryId != mission.CountryId)
            {
                throw new ArgumentException($"City with ID {mission.CityId} does not belong to Country with ID {mission.CountryId}.");
            }
        }

        #endregion
    }

    /// <summary>
    /// Mission statistics model
    /// </summary>
    public class MissionStatistics
    {
        public int TotalMissions { get; set; }
        public int ActiveMissions { get; set; }
        public int UpcomingMissions { get; set; }
        public int OngoingMissions { get; set; }
        public int CompletedMissions { get; set; }
    }
}