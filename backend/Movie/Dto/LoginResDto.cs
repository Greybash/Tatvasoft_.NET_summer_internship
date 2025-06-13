namespace Movies.Dto // Fixed namespace
{
    public class LoginResDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}