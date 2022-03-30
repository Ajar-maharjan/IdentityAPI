using AutoMapper;
using IdentityAPI.ActionFilters;
using IdentityAPI.Models;
using IdentityAPI.Models.DTO;
using IdentityAPI.Models.EmailSender;
using IdentityAPI.Models.Enum;
using IdentityAPI.Services.AuthService;
using IdentityAPI.Services.EmailSender;
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
        private readonly IEmailSender _emailSender;

        public AccountController(IMapper mapper, UserManager<User> userManager, ILogger<AccountController> logger, IAuthService authService, IEmailSender emailSender)
        {
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _authService = authService;
            _emailSender = emailSender;
        }
        [HttpPost("Register")]
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
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("URLfromclient", "Account", new { token, email = user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Confirmation email link", confirmationLink, null);
            await _emailSender.SendEmailAsync(message);
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
                return Unauthorized("Wrong username or password");
            }
            return Ok(new { Token = await _authService.CreateToken() });
        }

        [HttpPost("forgotpassword")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto userRequest)
        {
            var user = await _userManager.FindByEmailAsync(userRequest.Email);
            if (user == null)
            {
                return BadRequest("Invalid email");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action("URLfromclient", "Account", new { token, email = user.Email }, Request.Scheme);

            var message = new Message(new string[] { user.Email }, "Subject: Reset password token", callback , null);
            await _emailSender.SendEmailAsync(message);
            return Ok();
        }

        [HttpPost("resetpassword")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto userRequest)
        {
            var user = await _userManager.FindByEmailAsync(userRequest.Email);
            if (user == null)
                return BadRequest("Unable to find user");
            var resetPassResult = await _userManager.ResetPasswordAsync(user, userRequest.Token, userRequest.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            return Ok("Password has been reset");
        }

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest("Error");
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest("Error");
            return Ok("Email verified");
        }
    }
}
