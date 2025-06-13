namespace Movies.Dto // Fixed namespace
{
    public class MissionApplicationRequest
    {
        public int MissionId { get; set; }
        public int Seats { get; set; } = 1;
        public string? Message { get; set; }
    }
}