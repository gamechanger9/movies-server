using MoviesAPI.DTOs;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models
{
    public class Theater : IId
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        public required Point Location { get; set; }
    }
}
