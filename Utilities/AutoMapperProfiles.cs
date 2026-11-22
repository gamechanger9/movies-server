using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MoviesAPI.DTOs;
using MoviesAPI.Models;
using NetTopologySuite.Geometries;

namespace MoviesAPI.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            ConfigureGenres();
            ConfigureActors();
            ConfigureTheaters(geometryFactory);
            ConfigureMovies();
            ConfigureUsers();
        }

        private void ConfigureUsers()
        {
            CreateMap<IdentityUser, UserDTO>();
        }

        private void ConfigureMovies()
        {
            CreateMap<MovieCreationDTO, Movie>()
                .ForMember(movie => movie.Poster, options => options.Ignore())
                .ForMember(movie => movie.MoviesGenres, options => 
                options.MapFrom(p => p.GenresIds!.Select(id => new MoviesGenres { GenreId = id })))
                .ForMember(movie => movie.MoviesTheaters, options =>
                options.MapFrom(p => p.TheatersIds!.Select(id => new MoviesTheaters { TheaterId = id})))
                .ForMember(movie => movie.MoviesActors, options =>
                options.MapFrom(p => p.Actors!.Select(actor => new MoviesActors { ActorId = actor.Id, Character = actor.Character })));

            CreateMap<Movie, MovieDTO>();

            CreateMap<Movie, MovieDetailsDTO>()
                .ForMember(movie => movie.Genres, options => 
                options.MapFrom(m => m.MoviesGenres))
                .ForMember(movie => movie.Theaters, options =>
                options.MapFrom(m => m.MoviesTheaters))
                .ForMember(movie => movie.Actors, options =>
                options.MapFrom(m => m.MoviesActors.OrderBy(o => o.Order)));

            CreateMap<MoviesGenres, GenreDTO>()
                .ForMember(movie => movie.Id, options => 
                options.MapFrom(m => m.GenreId))
                .ForMember(movie => movie.Name, options =>
                options.MapFrom(m => m.Genre.Name));

            CreateMap<MoviesTheaters, TheaterDTO>()
                .ForMember(movie => movie.Id, options =>
                options.MapFrom(m => m.TheaterId))
                .ForMember(movie => movie.Name, options =>
                options.MapFrom(m => m.Theater.Name))
                .ForMember(movie => movie.Longitude, options =>
                options.MapFrom(m => m.Theater.Location.X))
                .ForMember(movie => movie.Latitude, options =>
                options.MapFrom(m => m.Theater.Location.Y));

            CreateMap<MoviesActors, MovieActorDTO>()
                .ForMember(movie => movie.Id, options =>
                options.MapFrom(m => m.ActorId))
                .ForMember(movie => movie.Name, options =>
                options.MapFrom(m => m.Actor.Name))
                .ForMember(movie => movie.Picture, options =>
                options.MapFrom(m => m.Actor.Picture));
        }

        private void ConfigureActors()
        {
            CreateMap<ActorCreationDTO, Actor>()
                .ForMember(actor => actor.Picture, options => options.Ignore());
            CreateMap<Actor, ActorDTO>();
            CreateMap<Actor, MovieActorDTO>();
        }

        private void ConfigureGenres()
        {
            CreateMap<GenreCreationDTO, Genre>();
            CreateMap<Genre, GenreDTO>();
        }

        private void ConfigureTheaters(GeometryFactory geometryFactory)
        {
            CreateMap<TheaterCreationDTO, Theater>()
                .ForMember(theater => theater.Location, options => options.MapFrom(dto => geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude))));
            CreateMap<Theater, TheaterDTO>()
                .ForMember(dto => dto.Latitude, options => options.MapFrom(theater => theater.Location.Y))
                .ForMember(dto => dto.Longitude, options => options.MapFrom(theater => theater.Location.X));
        }
    }
}
