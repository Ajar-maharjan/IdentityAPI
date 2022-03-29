using AutoMapper;
using IdentityAPI.ActionFilters;
using IdentityAPI.Models;
using IdentityAPI.Models.DTO;
using IdentityAPI.Models.Enum;
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

        public AccountController(IMapper mapper, UserManager<User> userManager, ILogger<AccountController> logger)
        {
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Register(UserRegistrationDto userModel)
        {
            var user = _mapper.Map<User>(userModel);
            var result = await _userManager.CreateAsync(user, userModel.Password);
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
    }
}
 