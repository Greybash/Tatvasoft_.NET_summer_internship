using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;
using System.Linq.Expressions;

namespace Movies.Services
{
    public class MissionThemeService
    {
        private readonly MoviesDbContext _context;

        public MissionThemeService(MoviesDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<MissionTheme> Themes, int TotalCount)> GetAllMissionThemesAsync(
            string? name = null,
            bool? isActive = null,
            string? sortBy = "name",
            bool sortDescending = false,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.MissionThemes.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(mt => mt.Name.ToLower().Contains(name.ToLower()));
            }

            if (isActive.HasValue)
            {
                query = query.Where(mt => mt.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, sortBy, sortDescending);

            // Apply pagination
            var themes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (themes, totalCount);
        }

        public async Task<MissionTheme?> GetMissionThemeByIdAsync(int id)
        {
            return await _context.MissionThemes
                .FirstOrDefaultAsync(mt => mt.Id == id);
        }

        public async Task<MissionTheme> CreateMissionThemeAsync(MissionTheme request, int userId)
        {
            var theme = new MissionTheme
            {
                Name = request.Name,
                IsActive = request.IsActive
            };

            _context.MissionThemes.Add(theme);
            await _context.SaveChangesAsync();

            return theme;
        }

        public async Task<MissionTheme?> UpdateMissionThemeAsync(int id, MissionTheme request, int userId)
        {
            var theme = await _context.MissionThemes.FirstOrDefaultAsync(mt => mt.Id == id);

            if (theme == null)
                return null;

            theme.Name = request.Name;
            theme.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return theme;
        }

        public async Task<bool> DeleteMissionThemeAsync(int id)
        {
            var theme = await _context.MissionThemes.FindAsync(id);
            if (theme == null)
                return false;

            _context.MissionThemes.Remove(theme);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MissionThemeExistsAsync(int id)
        {
            return await _context.MissionThemes.AnyAsync(mt => mt.Id == id);
        }

        public async Task<bool> MissionThemeNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.MissionThemes.Where(mt => mt.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(mt => mt.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<MissionTheme>> GetActiveMissionThemesAsync()
        {
            return await _context.MissionThemes
                .Where(mt => mt.IsActive)
                .OrderBy(mt => mt.Name)
                .ToListAsync();
        }

        private IQueryable<MissionTheme> ApplySorting(IQueryable<MissionTheme> query, string? sortBy, bool sortDescending)
        {
            Expression<Func<MissionTheme, object>> keySelector = sortBy?.ToLower() switch
            {
                "name" => mt => mt.Name,
                "isactive" => mt => mt.IsActive,
                "id" => mt => mt.Id,
                _ => mt => mt.Name
            };

            return sortDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}