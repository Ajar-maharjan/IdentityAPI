global using Serilog;
using IdentityAPI.ActionFilters;
using IdentityAPI.Data;
using IdentityAPI.Extensions;
using IdentityAPI.Middleware;
using IdentityAPI.Models;
using IdentityAPI.Services.AuthService;
using IdentityAPI.Services.EmailSender;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

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
builder.Services.ConfigureIdentity();
builder.Services.ConfigureEmailSender(Configuration);
builder.Services.ConfigureJWT(Configuration);
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureCors();
//builder.Services.ConfigureHeaderLogging();
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<TraceIPAttribute>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

var app = builder.Build();
app.UseHttpLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

//app.UseMiddleware<HttpRequestBodyMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"StaticFiles")),
    RequestPath = new PathString("/StaticFiles")
});
app.UseRouting();
app.UseCors(myAllowSpecificOrigins);
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
