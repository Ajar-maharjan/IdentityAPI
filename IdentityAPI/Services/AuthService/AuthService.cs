using IdentityAPI.Models;
using IdentityAPI.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityAPI.Services.AuthService
{
    public interface IAuthService
    {
        Task<bool> ValidateUser(UserAuthenticationDto userForAuth);
        Task<string> CreateToken();
    }
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private User _user;

        public AuthService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        public async Task<string> CreateToken()
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public async Task<bool> ValidateUser(UserAuthenticationDto userForAuth)
        {

            _user = await _userManager.FindByNameAsync(userForAuth.UserName);
            if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(_user))
                return false;
            if (_user != null && await _userManager.CheckPasswordAsync(_user,
            userForAuth.Password))
            {
                if (_userManager.SupportsUserLockout && await _userManager.GetAccessFailedCountAsync(_user) > 0)
                {
                    await _userManager.ResetAccessFailedCountAsync(_user);
                }
                return true;
            }
            else
            {
                if (_userManager.SupportsUserLockout && await _userManager.GetLockoutEnabledAsync(_user))
                {
                    await _userManager.AccessFailedAsync(_user);
                }
                return false;
            }
        }
        private SigningCredentials GetSigningCredentials()
        {
            var key =
                System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"));
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim>
         {
            new Claim(ClaimTypes.Email, _user.Email),
            new Claim(ClaimTypes.NameIdentifier,_user.Id),
            new Claim(ClaimTypes.Name, _user.UserName)
         };
            var roles = await _userManager.GetRolesAsync(_user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenOptions = new JwtSecurityToken
         (
             issuer: jwtSettings.GetSection("validIssuer").Value,
             audience: jwtSettings.GetSection("validAudience").Value,
             claims: claims,
             expires:DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("expires").Value)),
             signingCredentials: signingCredentials
         );
            return tokenOptions;
        }


    }
}