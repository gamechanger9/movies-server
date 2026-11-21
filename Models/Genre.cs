using MoviesAPI.DTOs;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models
{
    public class Genre : IId
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "You must fill the {0} field")]
        [StringLength(maximumLength: 50)]
        //[FirstLetterUpperCase]
        public required string Name { get; set; }
    }
}
