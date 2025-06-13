using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Services;

namespace Movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;

        public LocationController(LocationService locationService)
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Get all active countries
        /// </summary>
        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var countries = await _locationService.GetAllCountriesAsync();
                return Ok(countries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving countries.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get country by ID
        /// </summary>
        [HttpGet("countries/{id}")]
        public async Task<IActionResult> GetCountry(int id)
        {
            try
            {
                var country = await _locationService.GetCountryByIdAsync(id);

                if (country == null)
                    return NotFound(new { message = $"Country with ID {id} not found." });

                return Ok(country);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the country.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active cities for a specific country
        /// </summary>
        [HttpGet("countries/{countryId}/cities")]
        public async Task<IActionResult> GetCitiesByCountry(int countryId)
        {
            try
            {
                // First check if country exists
                if (!await _locationService.CountryExistsAsync(countryId))
                {
                    return NotFound(new { message = $"Country with ID {countryId} not found." });
                }

                var cities = await _locationService.GetCitiesByCountryAsync(countryId);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving cities.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active cities
        /// </summary>
        [HttpGet("cities")]
        public async Task<IActionResult> GetAllCities()
        {
            try
            {
                var cities = await _locationService.GetAllCitiesAsync();
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving cities.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get city by ID
        /// </summary>
        [HttpGet("cities/{id}")]
        public async Task<IActionResult> GetCity(int id)
        {
            try
            {
                var city = await _locationService.GetCityByIdAsync(id);

                if (city == null)
                    return NotFound(new { message = $"City with ID {id} not found." });

                return Ok(city);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the city.", error = ex.Message });
            }
        }
    }
}