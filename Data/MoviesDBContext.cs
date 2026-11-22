using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Models;

namespace MoviesAPI.Data
{
    public class MoviesDBContext : IdentityDbContext
    {
        public MoviesDBContext(DbContextOptions options): base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MoviesTheaters>()
                .HasKey(mt => new { mt.MovieId, mt.TheaterId });

            modelBuilder.Entity<MoviesActors>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });

            modelBuilder.Entity<MoviesGenres>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MoviesTheaters> MovieTheaters { get; set; }
        public DbSet<MoviesActors> MovieActors { get; set; }
        public DbSet<MoviesGenres> MovieGenres { get; set; }
        public DbSet<Rating> MovieRatings { get; set; }


        }
}
