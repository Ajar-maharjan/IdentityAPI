using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace IdentityAPI.Middleware
{
    public class HttpRequestBodyMiddleware
    {
        private readonly ILogger logger;
        private readonly RequestDelegate next;

        public HttpRequestBodyMiddleware(ILogger<HttpRequestBodyMiddleware> logger,
            RequestDelegate next)
        {
            this.logger = logger;
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            var reader = new StreamReader(context.Request.Body);

            string body = await reader.ReadToEndAsync();
            logger.LogInformation(
                $"Request {context.Request?.Method}: {context.Request?.Path.Value}\n{body}");

            context.Request.Body.Position = 0L;

            await next(context);
        }
    }
}
