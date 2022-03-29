using IdentityAPI.Data;
using IdentityAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityAPI.Extensions
{
    public static class IdentityExtension
    {
        public static void ConfigureIdentity(this IServiceCollection services)
        {

            var builder = services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 10;
                options.User.RequireUniqueEmail = true;
            });
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole),
                builder.Services);
            builder.AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();
        }
    }
}
