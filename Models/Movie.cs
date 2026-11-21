using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required]
        [StringLength(300)]
        public required string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        [Unicode(false)]
        public string? Poster { get; set; }
        public string? Trailer { get; set; }
        public List<MoviesGenres> MoviesGenres { get; set; } = new List<MoviesGenres>();
        public List<MoviesTheaters> MoviesTheaters { get; set; } = new List<MoviesTheaters>();
        public List<MoviesActors> MoviesActors { get; set; } = new List<MoviesActors>();

    }
}
