using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using MoviesAPI.Models;
using MoviesAPI.Services;
using MoviesAPI.Utilities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly MoviesDBContext _dBContext;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IFileStorage _fileStorage;
        private const string cacheTag = "actors";
        private readonly string container = "actors";

        public ActorsController(MoviesDBContext dBContext, IMapper mapper, IOutputCacheStore outputCacheStore, IFileStorage fileStorage)
        {
            _dBContext = dBContext;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            _fileStorage = fileStorage;
        }

        // GET: api/<ActorsController>
        [HttpGet]
        [OutputCache(Tags = [cacheTag])]
        public async Task<List<ActorDTO>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = _dBContext.Actors;
            await HttpContext.InsertPaginationParametersInHeaders(queryable);
            var actors = await queryable.OrderBy(x => x.Name).Paginate(paginationDTO).ToListAsync();
            return _mapper.Map<List<ActorDTO>>(actors);
        }

        // GET api/<ActorsController>/5
        [HttpGet("{id:int}", Name = "GetActorById")]
        [OutputCache(Tags = [cacheTag])]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            var actor = await _dBContext.Actors.FirstOrDefaultAsync(x => x.Id == id);
            if (actor == null)
            {
                return NotFound();
            }
            return _mapper.Map<ActorDTO>(actor);

        }

        // POST api/<ActorsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ActorCreationDTO actorCreationDTO)
        {
            var actor = _mapper.Map<Actor>(actorCreationDTO);

            if(actorCreationDTO.Picture != null)
            {
                var url = await _fileStorage.Store(container, actorCreationDTO.Picture);
                actor.Picture = url;
            }
            _dBContext.Add(actor);
            await _dBContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);
            var actorDTO = _mapper.Map<ActorDTO>(actor);
            return CreatedAtRoute("GetActorById", new { id = actor.Id }, actorDTO);
        }

        // PUT api/<ActorsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] ActorCreationDTO actorCreationDTO)
        {
            var actor = await _dBContext.Actors.FirstOrDefaultAsync(x => x.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            actor = _mapper.Map(actorCreationDTO, actor);

            if (actorCreationDTO.Picture != null)
            {
                actor.Picture = await _fileStorage.Edit(actor.Picture, container, actorCreationDTO.Picture);
            }

            await _dBContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);
            return NoContent();

        }

        // DELETE api/<ActorsController>/5
        [HttpDelete("{id}")]
        [OutputCache(Tags = [cacheTag])]
        public async Task<IActionResult> Delete(int id)
        {
            var actor = _dBContext.Actors.FirstOrDefault(x => x.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            _dBContext.Remove(actor);
            await _dBContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);
            await _fileStorage.Delete(actor.Picture, container);
            return NoContent();
        }
    }
}
