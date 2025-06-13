using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    public class MissionSkill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}