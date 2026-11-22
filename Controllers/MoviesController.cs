using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using MoviesAPI.Models;
using MoviesAPI.Services;
using MoviesAPI.Utilities;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "isadmin")]

    public class MoviesController : BaseController
    {
        private readonly MoviesDBContext _dBContext;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private const string cacheTag = "movies";
        private readonly string container = "movies";
        private readonly IUsersService _usersService;

        public MoviesController(MoviesDBContext dBContext, IOutputCacheStore outputCacheStore, IMapper mapper, IFileStorage fileStorage, IUsersService usersService) : base(dBContext, mapper, outputCacheStore, cacheTag)
        {
            _dBContext = dBContext;
            _outputCacheStore = outputCacheStore;
            _mapper = mapper;
            _fileStorage = fileStorage;
            _usersService = usersService;
        }

        [HttpGet("{id:int}", Name = "GetMovieById")]
        [OutputCache(Tags = [cacheTag])]
        [AllowAnonymous]
        public async Task<ActionResult<MovieDetailsDTO>> Get(int id)
        {
            var movie = await _dBContext.Movies
                .ProjectTo<MovieDetailsDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(m => m.Id == id);

            if(movie is null)
            {
                return NotFound();
            }

            var avgRate = 0.0;
            var userVote = 0;

            var movieRated = await _dBContext.MovieRatings.AnyAsync(mr => mr.MovieId == id);

            if (movieRated)
            {
                avgRate = await _dBContext.MovieRatings.Where(mr => mr.MovieId == id).AverageAsync(mr => mr.Rate);

                if (HttpContext.User.Identity!.IsAuthenticated)
                {
                    var userId = await _usersService.GetUserId();
                    var ratingsDB = await _dBContext.MovieRatings.FirstOrDefaultAsync(mr => mr.MovieId == id && mr.UserId == userId);

                    if(ratingsDB is not null)
                    {
                        userVote = ratingsDB.Rate;
                    }
                }
            }

            movie.AverageRate = avgRate;
            movie.UserVote = userVote;
            return movie;
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] MoviesFilterDTO moviesFilterDTO)
        {
            var moviesQueryable = _dBContext.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(moviesFilterDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(m => m.Title.Contains(moviesFilterDTO.Title));
            }

            if (moviesFilterDTO.InTheaters)
            {
                moviesQueryable = moviesQueryable.Where(m => m.MoviesTheaters.Select(mt => mt.MovieId).Contains(m.Id));
            }

            if (moviesFilterDTO.UpcomingReleases)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(m => m.ReleaseDate > today);
            }

            if(moviesFilterDTO.GenreId != 0)
            {
                moviesQueryable = moviesQueryable.Where(m => m.MoviesGenres.Select(mg => mg.GenreId).Contains(moviesFilterDTO.GenreId));
            }

            await HttpContext.InsertPaginationParametersInHeaders(moviesQueryable);

            var movies = await moviesQueryable.Paginate(moviesFilterDTO.PaginationDTO)
                .ProjectTo<MovieDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return movies;
        }

        [HttpGet("landing")]
        [OutputCache(Tags = [cacheTag])]
        [AllowAnonymous]
        public async Task<ActionResult<LandingDTO>> Landing()
        {
            var top = 6;
            var today = DateTime.Today;
            var upcomingReleases = await _dBContext.Movies
                .Where(m => m.ReleaseDate > today)
                .OrderBy(m => m.ReleaseDate)
                .Take(top)
                .ProjectTo<MovieDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
            var inTheaters = await _dBContext.Movies
                .Where(m => m.MoviesTheaters.Select(mt => mt.MovieId).Contains(m.Id))
                .OrderBy(m => m.ReleaseDate)
                .Take(top)
                .ProjectTo<MovieDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
            var landingDTO = new LandingDTO
            {
                UpcomingReleases = upcomingReleases,
                InTheaters = inTheaters
            };
            return landingDTO;
        }

        [HttpGet("postget")]
        public async Task<ActionResult<MoviesPostGetDTO>> PostGet()
        {
            var genres = await _dBContext.Genres.OrderBy(g => g.Name).ProjectTo<GenreDTO>(_mapper.ConfigurationProvider).ToListAsync();
            var theaters = await _dBContext.Theaters.OrderBy(t => t.Name).ProjectTo<TheaterDTO>(_mapper.ConfigurationProvider).ToListAsync();

            return new MoviesPostGetDTO { Genres = genres, Theaters = theaters };

        }

        [HttpPost]
        public async Task<CreatedAtRouteResult> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = _mapper.Map<Movie>(movieCreationDTO);
            if (movieCreationDTO.Poster != null)
            {
                var url = await _fileStorage.Store(container, movieCreationDTO.Poster);
                movie.Poster = url;
            }
            
            AssignActorsOrder(movie);
            _dBContext.Add(movie);
            await _dBContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);
            var movieDTO = _mapper.Map<MovieDTO>(movie);
            return new CreatedAtRouteResult("GetMovieById", new { id = movieDTO.Id }, movieDTO);
        }

        [HttpGet("putget/{id:int}")]
        public async Task<ActionResult<MoviesPutGetDTO>> PutGet(int id)
        {
            var movie = await _dBContext.Movies.ProjectTo<MovieDetailsDTO>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(m => m.Id == id);

            if(movie == null)
            {
                return NotFound();
            }

            var selectedGenresIds = movie.Genres.Select(g => g.Id).ToList();
            var nonSelectedGenres = await _dBContext.Genres.Where(g => !selectedGenresIds.Contains(g.Id)).ProjectTo<GenreDTO>(_mapper.ConfigurationProvider).ToListAsync();

            var selectedTheaterIds = movie.Theaters.Select(t => t.Id).ToList();
            var nonSelectedTheaters = await _dBContext.Theaters.Where(t => !selectedTheaterIds.Contains(t.Id)).ProjectTo<TheaterDTO>(_mapper.ConfigurationProvider).ToListAsync();

            var response = new MoviesPutGetDTO()
            {
                Movie = movie,
                SelectedGenres = movie.Genres,
                NonSelectedGenres = nonSelectedGenres,
                SelectedTheaters = movie.Theaters,
                NonSelectedTheaters = nonSelectedTheaters,
                Actors = movie.Actors,
            };

            return response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = await _dBContext.Movies.Include(m => m.MoviesActors).Include(m => m.MoviesGenres).Include(m => m.MoviesTheaters).FirstOrDefaultAsync(m => m.Id == id);

            if(movie is null)
            {
                return NotFound();
            }

            movie = _mapper.Map(movieCreationDTO, movie);

            if(movieCreationDTO.Poster != null)
            {
                movie.Poster = await _fileStorage.Edit(movie.Poster, container, movieCreationDTO.Poster);
            }

            AssignActorsOrder(movie);

            await _dBContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var movie = await _dBContext.Movies.FirstOrDefaultAsync(m => m.Id == id);

            if (movie is null)
            {
                return NotFound();
            }

            _dBContext.Remove(movie);
            await _dBContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);
            await _fileStorage.Delete(movie.Poster, container);

            return NoContent();
        }

        private void AssignActorsOrder(Movie movie)
        {
            if (movie.MoviesActors != null)
            {
                for (int i = 0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        }
    }
}
