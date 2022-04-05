global using Serilog;
using IdentityAPI.ActionFilters;
using IdentityAPI.Data;
using IdentityAPI.Extensions;
using IdentityAPI.Middleware;
using IdentityAPI.Models;
using IdentityAPI.Services.AuthService;
using IdentityAPI.Services.EmailSender;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
var Configuration = builder.Configuration;
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var conn = Configuration.GetConnectionString("DefaultConnection");
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddControllers(config =>
{
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
}).AddXmlDataContractSerializerFormatters();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationContext>(opts =>
        opts.UseSqlServer(conn));
builder.Services.AddIdentityCore<User>();
builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureEmailSender(Configuration);
builder.Services.ConfigureJWT(Configuration);
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureCors();
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<TraceIPAttribute>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
app.UseStaticFiles();
app.UseMiddleware<HttpRequestBodyMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(myAllowSpecificOrigins);
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllers();

app.Run();
