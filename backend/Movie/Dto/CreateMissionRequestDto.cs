using System.ComponentModel.DataAnnotations;

public class CreateMissionRequest
{
    [Required]
    [MaxLength(200)]
    public string MissionTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string MissionDescription { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string MissionOrganisationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string MissionOrganisationDetail { get; set; } = string.Empty;

    [Required]
    public int CountryId { get; set; }

    [Required]
    public int CityId { get; set; }

    [Required]
    public int MissionThemeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string MissionType { get; set; } = string.Empty;

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
}