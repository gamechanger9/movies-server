using AutoMapper;
using MoviesAPI.DTOs;
using MoviesAPI.Models;

namespace MoviesAPI.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles() 
        {
            ConfigureGenres();
            ConfigureActors();
        }

        private void ConfigureActors()
        {
            CreateMap<ActorCreationDTO, Actor>()
                .ForMember(actor => actor.Picture, options => options.Ignore());
            CreateMap<Actor, ActorDTO>();
        }

        private void ConfigureGenres()
        {
            CreateMap<GenreCreationDTO, Genre>();
            CreateMap<Genre, GenreDTO>();
        }
    }
}
