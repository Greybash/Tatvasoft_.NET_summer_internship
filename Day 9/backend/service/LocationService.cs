using Microsoft.EntityFrameworkCore;
using Movies.DataAccess;
using Movies.Models;

namespace Movies.Services
{
    public class LocationService
    {
        private readonly MoviesDbContext _context;

        public LocationService(MoviesDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all active countries
        /// </summary>
        /// <returns>List of active countries</returns>
        public async Task<IEnumerable<Country>> GetAllCountriesAsync()
        {
            return await _context.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get country by ID
        /// </summary>
        /// <param name="id">Country ID</param>
        /// <returns>Country if found, otherwise null</returns>
        public async Task<Country?> GetCountryByIdAsync(int id)
        {
            return await _context.Countries
                .Where(c => c.Id == id && c.IsActive)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get all active cities for a specific country
        /// </summary>
        /// <param name="countryId">Country ID</param>
        /// <returns>List of active cities for the specified country</returns>
        public async Task<IEnumerable<City>> GetCitiesByCountryAsync(int countryId)
        {
            return await _context.Cities
                .Include(c => c.Country)
                .Where(c => c.CountryId == countryId && c.IsActive && c.Country.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get all active cities
        /// </summary>
        /// <returns>List of all active cities</returns>
        public async Task<IEnumerable<City>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Include(c => c.Country)
                .Where(c => c.IsActive && c.Country.IsActive)
                .OrderBy(c => c.Country.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get city by ID
        /// </summary>
        /// <param name="id">City ID</param>
        /// <returns>City if found, otherwise null</returns>
        public async Task<City?> GetCityByIdAsync(int id)
        {
            return await _context.Cities
                .Include(c => c.Country)
                .Where(c => c.Id == id && c.IsActive && c.Country.IsActive)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Check if a country exists and is active
        /// </summary>
        /// <param name="countryId">Country ID</param>
        /// <returns>True if country exists and is active, otherwise false</returns>
        public async Task<bool> CountryExistsAsync(int countryId)
        {
            return await _context.Countries
                .AnyAsync(c => c.Id == countryId && c.IsActive);
        }

        /// <summary>
        /// Check if a city exists, is active, and belongs to the specified country
        /// </summary>
        /// <param name="cityId">City ID</param>
        /// <param name="countryId">Country ID</param>
        /// <returns>True if city exists, is active, and belongs to the country, otherwise false</returns>
        public async Task<bool> CityExistsInCountryAsync(int cityId, int countryId)
        {
            return await _context.Cities
                .AnyAsync(c => c.Id == cityId && c.CountryId == countryId && c.IsActive);
        }
    }
}