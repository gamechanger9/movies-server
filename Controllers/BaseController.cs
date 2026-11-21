using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using MoviesAPI.Utilities;
using System.Linq.Expressions;

namespace MoviesAPI.Controllers
{
    public class BaseController : ControllerBase
    {
        private readonly MoviesDBContext context;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly string _cacheTag;

        public BaseController(MoviesDBContext context, IMapper mapper, IOutputCacheStore outputCacheStore, string cacheTag)
        {
            this.context = context;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            _cacheTag = cacheTag;
        }

        protected async Task<List<TDTO>> Get<T, TDTO>(PaginationDTO pagination, Expression<Func<T, object>> orderBy) where T : class
        {
            var queryable = context.Set<T>().AsQueryable();
            await HttpContext.InsertPaginationParametersInHeaders(queryable);
            return await queryable
                .OrderBy(orderBy)
                .Paginate(pagination)
                .ProjectTo<TDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        protected async Task<ActionResult<TDTO>> GetById<T, TDTO>(int id)
            where T : class
            where TDTO : IId
        {
            var entity = await context.Set<T>()
                                .ProjectTo<TDTO>(_mapper.ConfigurationProvider)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return NotFound();
            }
            return entity;
        }

        protected async Task<CreatedAtRouteResult> Post<T, TCreationDTO, TDTO>(TCreationDTO creationDTO, string routeName)
            where T : class
            where TCreationDTO : class
            where TDTO : IId
        {
            var entity = _mapper.Map<T>(creationDTO);
            context.Add(entity);
            await context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cacheTag, default);
            var dto = _mapper.Map<TDTO>(entity);
            return CreatedAtRoute(routeName, new { id = dto.Id }, dto);
        }

        protected async Task<IActionResult> Put<T, TCreationDTO>(int id, TCreationDTO creationDTO)
            where T : class, IId
            where TCreationDTO : class
        {
            var entity = await context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return NotFound();
            }
            _mapper.Map(creationDTO, entity);
            await context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cacheTag, default);
            return NoContent();
        }

        protected async Task<IActionResult> Delete<T>(int id) where T : class, IId
        {
            var recordsDeleted = await context.Set<T>().Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
            {
                return NotFound();
            }
            await _outputCacheStore.EvictByTagAsync(_cacheTag, default);
            return NoContent();
        }
        protected async Task<List<TDTO>> Get<T, TDTO>(Expression<Func<T, object>> orderBy) where T : class
        {
            var queryable = context.Set<T>().AsQueryable();
            return await queryable
                .OrderBy(orderBy)
                .ProjectTo<TDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

    }
}
