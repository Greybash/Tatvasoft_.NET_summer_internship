using System.ComponentModel.DataAnnotations;

namespace Movies.Dto // Fixed namespace
{
    public class LoginReqDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}