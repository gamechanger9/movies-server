using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using MoviesAPI.Models;
using MoviesAPI.Services;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/ratings")]
    public class RatingsController : ControllerBase
    {
        private readonly MoviesDBContext _dbContext;
        private readonly IUsersService _usersService;

        public RatingsController(MoviesDBContext moviesDBContext, IUsersService usersService)
        {
            _dbContext = moviesDBContext;
            _usersService = usersService;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] RatingCreationDTO ratingCreationDTO)
        {
            var movie = await _dbContext.Movies.AnyAsync(m => m.Id == ratingCreationDTO.MovieId);

            if (!movie)
            {
                return NotFound();
            }

            var userId = await _usersService.GetUserId();
            var existingRating = await _dbContext.MovieRatings.FirstOrDefaultAsync(mr => mr.MovieId == ratingCreationDTO.MovieId && mr.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Rate = ratingCreationDTO.Rate;
            }
            else
            {
                var rating = new Rating() { UserId = userId, Rate = ratingCreationDTO.Rate, MovieId = ratingCreationDTO.MovieId };
                _dbContext.Add(rating);
            }

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
