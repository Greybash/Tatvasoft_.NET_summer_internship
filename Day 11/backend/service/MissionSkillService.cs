using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;
using System.Linq.Expressions;

namespace Movies.Services
{
    public class MissionSkillService
    {
        private readonly MoviesDbContext _context;

        public MissionSkillService(MoviesDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<MissionSkill> Skills, int TotalCount)> GetAllMissionSkillsAsync(
            string? name = null,
            bool? isActive = null,
            string? sortBy = "name",
            bool sortDescending = false,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.MissionSkills.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(ms => ms.Name.ToLower().Contains(name.ToLower()));
            }

            if (isActive.HasValue)
            {
                query = query.Where(ms => ms.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, sortBy, sortDescending);

            // Apply pagination
            var skills = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (skills, totalCount);
        }

        public async Task<MissionSkill?> GetMissionSkillByIdAsync(int id)
        {
            return await _context.MissionSkills
                .FirstOrDefaultAsync(ms => ms.Id == id);
        }

        public async Task<MissionSkill> CreateMissionSkillAsync(MissionSkill request, int userId)
        {
            var skill = new MissionSkill
            {
                Name = request.Name,
                IsActive = request.IsActive
            };

            _context.MissionSkills.Add(skill);
            await _context.SaveChangesAsync();

            return skill;
        }

        public async Task<MissionSkill?> UpdateMissionSkillAsync(int id, MissionSkill request, int userId)
        {
            var skill = await _context.MissionSkills.FirstOrDefaultAsync(ms => ms.Id == id);

            if (skill == null)
                return null;

            skill.Name = request.Name;
            skill.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return skill;
        }

        public async Task<bool> DeleteMissionSkillAsync(int id)
        {
            var skill = await _context.MissionSkills.FindAsync(id);
            if (skill == null)
                return false;

            _context.MissionSkills.Remove(skill);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MissionSkillExistsAsync(int id)
        {
            return await _context.MissionSkills.AnyAsync(ms => ms.Id == id);
        }

        public async Task<bool> MissionSkillNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.MissionSkills.Where(ms => ms.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(ms => ms.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<MissionSkill>> GetActiveMissionSkillsAsync()
        {
            return await _context.MissionSkills
                .Where(ms => ms.IsActive)
                .OrderBy(ms => ms.Name)
                .ToListAsync();
        }

        private IQueryable<MissionSkill> ApplySorting(IQueryable<MissionSkill> query, string? sortBy, bool sortDescending)
        {
            Expression<Func<MissionSkill, object>> keySelector = sortBy?.ToLower() switch
            {
                "name" => ms => ms.Name,
                "isactive" => ms => ms.IsActive,
                "id" => ms => ms.Id,
                _ => ms => ms.Name
            };

            return sortDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}