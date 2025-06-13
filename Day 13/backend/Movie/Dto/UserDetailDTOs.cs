using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    /// <summary>
    /// DTO for creating user detail
    /// </summary>
    public class CreateUserDetailRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Surname { get; set; } = string.Empty;

        [MaxLength(50)]
        public string EmployeeId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Manager { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string MyProfile { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string WhyIVolunteer { get; set; } = string.Empty;

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int CityId { get; set; }

        [MaxLength(50)]
        public string Availability { get; set; } = string.Empty;

        [MaxLength(200)]
        public string LinkedInUrl { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string MySkills { get; set; } = string.Empty;

        public bool Status { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating user detail
    /// </summary>
    public class UpdateUserDetailRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Surname { get; set; } = string.Empty;

        [MaxLength(50)]
        public string EmployeeId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Manager { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string MyProfile { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string WhyIVolunteer { get; set; } = string.Empty;

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int CityId { get; set; }

        [MaxLength(50)]
        public string Availability { get; set; } = string.Empty;

        [MaxLength(200)]
        public string LinkedInUrl { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string MySkills { get; set; } = string.Empty;

        public bool Status { get; set; } = true;
    }

    /// <summary>
    /// Form request model for creating user detail with image upload
    /// </summary>
    public class CreateUserDetailFormRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Surname { get; set; } = string.Empty;

        [MaxLength(50)]
        public string EmployeeId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Manager { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string MyProfile { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string WhyIVolunteer { get; set; } = string.Empty;

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int CityId { get; set; }

        [MaxLength(50)]
        public string Availability { get; set; } = string.Empty;

        [MaxLength(200)]
        public string LinkedInUrl { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string MySkills { get; set; } = string.Empty;

        public bool Status { get; set; } = true;
    }

    /// <summary>
    /// Form request model for updating user detail with image upload
    /// </summary>
    public class UpdateUserDetailFormRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Surname { get; set; } = string.Empty;

        [MaxLength(50)]
        public string EmployeeId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Manager { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string MyProfile { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string WhyIVolunteer { get; set; } = string.Empty;

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int CityId { get; set; }

        [MaxLength(50)]
        public string Availability { get; set; } = string.Empty;

        [MaxLength(200)]
        public string LinkedInUrl { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string MySkills { get; set; } = string.Empty;

        public bool Status { get; set; } = true;
    }
}