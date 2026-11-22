using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.IdentityModel.Tokens;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "isadmin")]

    public class UsersController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly MoviesDBContext _dBContext;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IMapper _mapper;
        private const string cacheTag = "users";

        public UsersController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, MoviesDBContext dBContext, IOutputCacheStore outputCacheStore, IMapper mapper)
            : base(dBContext,mapper, outputCacheStore, cacheTag)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dBContext = dBContext;
            _outputCacheStore = outputCacheStore;
            _mapper = mapper;
        }

        [HttpGet("usersList")]
        [OutputCache(Tags = [cacheTag])]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserDTO>>> GetUsers([FromQuery] PaginationDTO paginationDTO)
        {
            return await Get<IdentityUser, UserDTO>(paginationDTO, orderBy: u => u.Email!);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponseDTO>> Register(UserCredentialsDTO userCredentialsDTO)
        {
            var user = new IdentityUser
            {
                UserName = userCredentialsDTO.Email,
                Email = userCredentialsDTO.Email,
            };

            var result = await _userManager.CreateAsync(user, userCredentialsDTO.Password);

            if (result.Succeeded)
            {
                await _outputCacheStore.EvictByTagAsync(cacheTag, default);
                return await BuildToken(user);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponseDTO>> Login(UserCredentialsDTO userCredentialsDTO)
        {
            var user = await _userManager.FindByEmailAsync(userCredentialsDTO.Email);

            if(user is null)
            {
                var errors = BuildeIncorrectLoginErrorMessage();
                return BadRequest(errors);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, userCredentialsDTO.Password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await BuildToken(user);
            }
            else
            {
                var errors = BuildeIncorrectLoginErrorMessage();
                return BadRequest(errors);
            }
        }

        [HttpPost("makeAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> MakeAdmin(EditClaimDTO editClaimDTO)
        {
            var user = await _userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                return NotFound();
            }

            await _userManager.AddClaimAsync(user, new Claim("isadmin", "true"));
            return NoContent();
        }

        [HttpPost("removeAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveAdmin(EditClaimDTO editClaimDTO)
        {
            var user = await _userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                return NotFound();
            }

            await _userManager.RemoveClaimAsync(user, new Claim("isadmin", "true"));
            return NoContent();
        }

        private IEnumerable<IdentityError> BuildeIncorrectLoginErrorMessage()
        {
            var identityError = new IdentityError() { Description = "Incorrect Login!"};
            var errors = new List<IdentityError>();
            errors.Add(identityError);
            return errors;
        }

        private async Task<AuthenticationResponseDTO> BuildToken(IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new("email", user.UserName!),
                new("userName", user.UserName!),
                new("cliendId", "257"),
            };

            var claimDB = await _userManager.GetClaimsAsync(user);

            claims.AddRange(claimDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiration, signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthenticationResponseDTO { Token = token, Expiration = expiration };
        }
    }
}
