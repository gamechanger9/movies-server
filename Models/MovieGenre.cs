namespace MoviesAPI.Models
{
    public class MoviesGenres
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;
    }
}