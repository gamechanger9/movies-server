using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models
{
    public class MoviesActors
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        public int ActorId { get; set; }
        public Actor Actor { get; set; } = null!;
        [StringLength(300)]
        public required string Character { get; set; }
        public int Order { get; set; }
    }
}
