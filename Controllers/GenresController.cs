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
    public class GenresController : ControllerBase
    {
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly MoviesDBContext _dbContext;
        private readonly IMapper _mapper;
        private const string cacheTag = "genres";

        public GenresController(IOutputCacheStore outputCacheStore, MoviesDBContext dBContext, IMapper mapper)
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
            var genre = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Id == id);
            if (genre == null) {
                return NotFound();
            }
            var genreDTO = _mapper.Map<GenreDTO>(genre);
            return genreDTO;
        }

        [HttpGet]
        [OutputCache(Tags = [cacheTag])]
        public async Task<List<GenreDTO>> GetAll([FromQuery] PaginationDTO pagination)
        {
            var queryable = _dbContext.Genres;
            await HttpContext.InsertPaginationParametersInHeaders(queryable);
            return await queryable
                .OrderBy(g => g.Name)
                .Paginate(pagination).ProjectTo<GenreDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpPost]
        public async Task<CreatedAtRouteResult> Post([FromBody] GenreCreationDTO genreCreationDTO)
        {
            var genre = _mapper.Map<Genre>(genreCreationDTO);
            _dbContext.Add(genre);
            await _dbContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);
            var genreDTO = _mapper.Map<GenreDTO>(genre);
            return CreatedAtRoute("GetGenreById", new { id = genre.Id }, genreDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] GenreCreationDTO genreCreationDTO)
        {
            var genre = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Id == id);
            if (genre == null) {
                return NotFound();
            }
            _mapper.Map(genreCreationDTO, genre);
            await _dbContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var recordsDeleted = await _dbContext.Genres.Where(g => g.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0) {
                return NotFound();
            }
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);
            return NoContent();
        }
    }
}
