using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Director { get; set; } = string.Empty;

        [Range(1800, 2100)]
        public int Year { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(50)]
        public string Genre { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        
        public int? UserId { get; set; }

        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}