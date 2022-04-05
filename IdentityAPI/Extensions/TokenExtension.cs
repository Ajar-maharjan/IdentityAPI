using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAPI.Extensions
{
    public static class TokenExtension
    {
        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = Environment.GetEnvironmentVariable("SECRET");
            if (secretKey is null)
            {
                throw new KeyNotFoundException("Environment not set");
            }
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.GetSection("validIssuer").Value,
                    ValidAudience = jwtSettings.GetSection("validAudience").Value,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new
                    SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey))
                };
            });
            //.AddGoogle("google", options =>
            //{
            //    var googleAuth = configuration.GetSection("Authentication:Google");
            //    options.ClientId = googleAuth["ClientId"];
            //    options.ClientSecret = googleAuth["ClientSecret"];
            //    options.SignInScheme = IdentityConstants.ExternalScheme;
            //    options.SaveTokens = true;
            //});

        }
    }

}
