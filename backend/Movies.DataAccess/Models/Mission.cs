using Movies.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Movies.Models
{
    public class Mission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string MissionTitle { get; set; }

        [Required]
        [MaxLength(2000)]
        public string MissionDescription { get; set; }

        [Required]
        [MaxLength(200)]
        public string MissionOrganisationName { get; set; }

        [MaxLength(1000)]
        public string? MissionOrganisationDetail { get; set; }

        // Foreign key properties
        [Required]
        public int CountryId { get; set; }

        [Required]
        public int CityId { get; set; }

        [Required]
        public int MissionThemeId { get; set; }

        // Navigation properties with foreign key attributes
        [ForeignKey(nameof(CountryId))]
        public virtual Country Country { get; set; } = null!;

        [ForeignKey(nameof(CityId))]
        public virtual City City { get; set; } = null!;

        [ForeignKey(nameof(MissionThemeId))]
        public virtual MissionTheme MissionTheme { get; set; } = null!;

        // Other properties
        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string MissionType { get; set; }

        public int? TotalSheets { get; set; }

        public DateTime? RegistrationDeadLine { get; set; }

        public int? MissionSkillId { get; set; }

        [MaxLength(2000)]
        public string? MissionImages { get; set; }

        [MaxLength(2000)]
        public string? MissionDocuments { get; set; }

        [MaxLength(50)]
        public string? MissionAvailability { get; set; }

        [MaxLength(500)]
        public string? MissionVideoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<MissionApp> MissionApps { get; set; } = new List<MissionApp>();
    }
}