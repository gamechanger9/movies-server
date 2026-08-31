using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class GenreCreationDTO
    {
        [Required(ErrorMessage = "You must fill the {0} field")]
        [StringLength(maximumLength: 50)]
        //[FirstLetterUpperCase]
        public required string Name { get; set; }
    }
}
