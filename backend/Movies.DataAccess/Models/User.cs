using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Models
{
    public class User
    {
        public string Email;

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
        public virtual ICollection<MissionApp> MissionApps { get; set; } = new List<MissionApp>();
        public virtual ICollection<UserDetail> UserDetails { get; set; } = new List<UserDetail>();

    }
}