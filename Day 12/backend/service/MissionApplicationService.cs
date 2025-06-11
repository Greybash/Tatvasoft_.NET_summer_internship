using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;


namespace Movies.Services
{
    public class MissionApplicationService
    {
        private readonly MoviesDbContext _context;

        public MissionApplicationService(MoviesDbContext context)
        {
            _context = context;
        }

        // User applies for a mission
        public async Task<(bool Success, int ApplicationId, string? ErrorMessage)> ApplyForMissionAsync(int userId, MissionApplicationRequest request)
        {
            var mission = await _context.Missions.FirstOrDefaultAsync(m => m.Id == request.MissionId && m.IsActive);
            if (mission == null)
                return (false, 0, "Mission not found or is inactive.");

            var existingApp = await _context.MissionApps.FirstOrDefaultAsync(a => a.MissionId == request.MissionId && a.UserId == userId);
            if (existingApp != null)
                return (false, 0, "You have already applied for this mission.");

            var app = new MissionApp
            {
                MissionId = request.MissionId,
                UserId = userId,
                AppliedDate = DateTime.UtcNow,
                Seats = request.Seats,
                Status = false,
                applystatus = true
            };

            _context.MissionApps.Add(app);
            await _context.SaveChangesAsync();

            return (true, app.Id, null);
        }

        public async Task<IEnumerable<MissionApp>> GetUserApplicationsAsync(int userId)
        {
            return await _context.MissionApps
                .Include(a => a.Mission)

                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CancelApplicationAsync(int applicationId, int userId)
        {
            var application = await _context.MissionApps.FirstOrDefaultAsync(a => a.Id == applicationId && a.UserId == userId);

            if (application == null)
                return (false, "Application not found.");

            if (application.Status)  // Already approved
                return (false, "Cannot cancel an approved application.");

            _context.MissionApps.Remove(application);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<IEnumerable<MissionApp>> GetPendingApplicationsAsync(int page, int pageSize)
        {
            return await _context.MissionApps
                .Include(a => a.Mission)
                .Include(a => a.User)
                .Where(a => !a.Status)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<MissionApp>> GetAllApplicationsAsync(int page, int pageSize, string? status, int? missionId)
        {
            var query = _context.MissionApps.Include(a => a.Mission)
                .Include(a => a.User).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "approved") query = query.Where(a => a.Status);
                else if (status == "pending") query = query.Where(a => !a.Status);
            }

            if (missionId.HasValue)
            {
                query = query.Where(a => a.MissionId == missionId.Value);
            }

            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> ApproveApplicationAsync(int applicationId, int adminId, string? comments)
        {
            var app = await _context.MissionApps.FirstOrDefaultAsync(a => a.Id == applicationId);
            if (app == null)
                return (false, "Application not found.");

            app.Status = true;
            app.applystatus = true;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RejectApplicationAsync(int applicationId, int adminId, string? comments)
        {
            var app = await _context.MissionApps.FirstOrDefaultAsync(a => a.Id == applicationId);
            if (app == null)
                return (false, "Application not found.");

            app.Status = false;
            app.applystatus = false;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<object> GetApplicationStatisticsAsync()
        {
            var total = await _context.MissionApps.CountAsync();
            var approved = await _context.MissionApps.CountAsync(a => a.Status);
            var pending = total - approved;

            return new
            {
                totalApplications = total,
                approvedApplications = approved,
                pendingApplications = pending
            };
        }

        public int GetTotalApplicationCount()
        {
            throw new NotImplementedException();
        }

        
    }


// Response Models
public class ApplicationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? ApplicationId { get; set; }
    }

    public class UserApplicationResponse
    {
        public int Id { get; set; }
        public int MissionId { get; set; }
        public string MissionTitle { get; set; }
        public string MissionDescription { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string MissionThemeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; }
        public int Seats { get; set; }
        public string? MissionImages { get; set; }
    }

    public class AdminApplicationResponse
    {
        public int Id { get; set; }
        public int MissionId { get; set; }
        public string MissionTitle { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; }
        public int Seats { get; set; }
        public DateTime MissionStartDate { get; set; }
        public DateTime? MissionEndDate { get; set; }
    }

    public class PaginatedApplicationResponse
    {
        public List<AdminApplicationResponse> Applications { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ApplicationStatistics
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int TotalSeatsApplied { get; set; }
        public int TotalSeatsApproved { get; set; }
    }

    // Request Models (from controller)
    public class MissionApplicationRequest
    {
        public int MissionId { get; set; }
        public int Seats { get; set; } = 1;
        public string? Message { get; set; }
    }
}