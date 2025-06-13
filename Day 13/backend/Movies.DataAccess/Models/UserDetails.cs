using Movies.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Movies.Models
{
    
    public class UserDetail
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string MyProfile { get; set; } = string.Empty;
        public string WhyIVolunteer { get; set; } = string.Empty;

        public int CountryId { get; set; }
        public int CityId { get; set; }

        public string Availability { get; set; } = string.Empty;
        public string LinkedInUrl { get; set; } = string.Empty;
        public string MySkills { get; set; } = string.Empty;
        public string UserImage { get; set; } = string.Empty;

        public bool Status { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(CountryId))]
        public virtual Country Country { get; set; } = null!;

        [ForeignKey(nameof(CityId))]
        public virtual City City { get; set; } = null!;
    }
}
