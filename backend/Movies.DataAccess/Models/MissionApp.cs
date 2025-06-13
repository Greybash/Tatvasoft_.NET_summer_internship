using Movies.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Movies.Models
{
    
        
        public class MissionApp
        {
            [Key]
            public int Id { get; set; }

            public int MissionId { get; set; }

            public int UserId { get; set; }

            public DateTime AppliedDate { get; set; }

            public bool Status { get; set; }
           public bool applystatus { get; set; } = false;

        public int Seats { get; set; }

            [ForeignKey(nameof(MissionId))]
            public virtual Mission Mission { get; set; } = null!;

            [ForeignKey(nameof(UserId))]
            public virtual User User { get; set; } = null!;
        }
    }


