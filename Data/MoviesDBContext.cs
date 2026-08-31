using Microsoft.EntityFrameworkCore;
using MoviesAPI.Models;

namespace MoviesAPI.Data
{
    public class MoviesDBContext : DbContext
    {
        public MoviesDBContext(DbContextOptions options): base(options)
        {
            
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
        }
}
