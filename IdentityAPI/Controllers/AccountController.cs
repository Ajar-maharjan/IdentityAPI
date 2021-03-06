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
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;

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
        private readonly IConfiguration _configuration;
        public AccountController(IMapper mapper, UserManager<User> userManager, ILogger<AccountController> logger, IAuthService authService, IEmailSender emailSender, IConfiguration configuration)
        {
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _authService = authService;
            _emailSender = emailSender;
            _configuration = configuration;
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
            var encodedEmailToken = System.Text.Encoding.UTF8.GetBytes(token);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);
            string confirmationLink = $"{_configuration["AppUrl"]}/api/account/confirmemail?user-Id-{user.Id}&token+{validEmailToken}";

            var message = new Message(new string[] { user.Email }, "Confirm your email", $"Confirm your email by <a href = '{confirmationLink}'> clicking here </a>.", null);
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
            var encodedEmailToken = System.Text.Encoding.UTF8.GetBytes(token);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);
            string resetLink = $"{_configuration["AppUrl"]}/api/account/resetpassword?userId-{user.Id}&token+{validEmailToken}";

            var message = new Message(new string[] { user.Email }, "Reset your password", $"reset your password  by <a href = '{resetLink}'> clicking here </a>." , null);
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
            var decodedToken = WebEncoders.Base64UrlDecode(userRequest.Token);
            var normalToken = System.Text.Encoding.UTF8.GetString(decodedToken);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, normalToken, userRequest.Password);
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
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(id))
                return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return BadRequest("Error");
            var decodedToken = WebEncoders.Base64UrlDecode(token);
            var normalToken = System.Text.Encoding.UTF8.GetString(decodedToken);
            var result = await _userManager.ConfirmEmailAsync(user, normalToken);
            if (!result.Succeeded)
                return BadRequest("Error");
            return Ok("Email verified");
        }

        [HttpPost("upload"), DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            var formCollection = await Request.ReadFormAsync();
            var files = formCollection.Files;
            var folderName = Path.Combine("Resources", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (files.Any(f => f.Length == 0))
            {
                return BadRequest();
            }
            foreach (var file in files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine(folderName, fileName); //you can add this path to a list and then return all dbPaths to the client if require
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return Ok("All the files are successfully uploaded.");

        }

        [HttpPost("upload/single"), DisableRequestSizeLimit]
        public async Task<IActionResult> SingleUpload()
        {
            var formCollection = await Request.ReadFormAsync();
            var file = formCollection.Files[0];
            var folderName = Path.Combine("Resources", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (file.Length > 0)
            {
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine(folderName, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return Ok(new { dbPath });

            }
            else
            {
                return BadRequest();
            }

        }
    }
}
