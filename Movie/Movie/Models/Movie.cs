namespace movies.Models
{
    public class Movie
    {
        public int Id { get; set; } 
        public string Title { get; set; }
        public string Director { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; }
    }
}
