using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using MoviesAPI.Models;

namespace MoviesAPI.Controllers
{
    [Route("api/theaters")]
    [ApiController]
    public class TheatersController : BaseController
    {
        private readonly MoviesDBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cacheTag = "theaters";

        public TheatersController(MoviesDBContext dbContext, IMapper mapper, IOutputCacheStore outputCacheStore) : base(dbContext, mapper, outputCacheStore, cacheTag)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet("{id:int}", Name = "GetTheaterById")]
        [OutputCache(Tags = [cacheTag])]
        public async Task<ActionResult<TheaterDTO>> Get(int id)
        {
            return await GetById<Theater, TheaterDTO>(id);
        }

        [HttpGet]
        [OutputCache(Tags = [cacheTag])]
        public async Task<List<TheaterDTO>> GetAll([FromQuery] PaginationDTO pagination)
        {
            return await Get<Theater, TheaterDTO>(pagination, orderBy: g => g.Name);
        }

        [HttpPost]
        public async Task<CreatedAtRouteResult> Post([FromBody] TheaterCreationDTO theaterCreationDTO)
        {
            return await Post<Theater, TheaterCreationDTO, TheaterDTO>(theaterCreationDTO, "GetTheaterById");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] TheaterCreationDTO theaterCreationDTO)
        {
            return await Put<Theater, TheaterCreationDTO>(id, theaterCreationDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await Delete<Theater>(id);
        }
    }
}
