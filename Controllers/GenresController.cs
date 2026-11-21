using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using MoviesAPI.Models;
using MoviesAPI.Utilities;
using System.Threading.Tasks;

namespace MoviesAPI.Controllers
{
    [Route("api/genres")]
    [ApiController]
    public class GenresController : BaseController
    {
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly MoviesDBContext _dbContext;
        private readonly IMapper _mapper;
        private const string cacheTag = "genres";

        public GenresController(IOutputCacheStore outputCacheStore, MoviesDBContext dBContext, IMapper mapper) : base(dBContext, mapper, outputCacheStore, cacheTag)
        {
            _outputCacheStore = outputCacheStore;
            _dbContext = dBContext;
            _mapper = mapper;
        }

        // GET: GenresController/Details/5
        [HttpGet("{id:int}", Name = "GetGenreById")]
        [OutputCache(Tags = [cacheTag])]
        public async Task<ActionResult<GenreDTO>> Get(int id)
        {
            return await GetById<Genre, GenreDTO>(id);
        }

        [HttpGet]
        [OutputCache(Tags = [cacheTag])]
        public async Task<List<GenreDTO>> GetAll([FromQuery] PaginationDTO pagination)
        {
            return await Get<Genre, GenreDTO>(pagination, orderBy: g => g.Name);
        }

        [HttpGet("all")]
        [OutputCache(Tags = [cacheTag])]
        public async Task<List<GenreDTO>> Get()
        {
            return await Get<Genre, GenreDTO>(orderBy: g => g.Name);
        }

        [HttpPost]
        public async Task<CreatedAtRouteResult> Post([FromBody] GenreCreationDTO genreCreationDTO)
        {
            return await Post<Genre, GenreCreationDTO, GenreDTO>(genreCreationDTO, "GetGenreById");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] GenreCreationDTO genreCreationDTO)
        {
            return await Put<Genre, GenreCreationDTO>(id, genreCreationDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await Delete<Genre>(id);
        }
    }
}
