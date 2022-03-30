using AutoMapper;
using IdentityAPI.ActionFilters;
using IdentityAPI.Models;
using IdentityAPI.Models.DTO;
using IdentityAPI.Models.Enum;
using IdentityAPI.Services.AuthService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthService _authService;

        public AccountController(IMapper mapper, UserManager<User> userManager, ILogger<AccountController> logger, IAuthService authService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _authService = authService;
        }
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Register(UserRegistrationDto userRequest)
        {
            var user = _mapper.Map<User>(userRequest);
            var result = await _userManager.CreateAsync(user, userRequest.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            await _userManager.AddToRoleAsync(user, Roles.Visitor.ToString());
            return StatusCode(201);

        }

        [HttpPost("login")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Authenticate(UserAuthenticationDto userRequest)
        {
            if (!await _authService.ValidateUser(userRequest))
            {
                _logger.LogWarning($"{nameof(Authenticate)}: Authentication failed. Wrong user name or password.");
                return Unauthorized();
            }
            return Ok(new { Token = await _authService.CreateToken() });
        }
    }
}
