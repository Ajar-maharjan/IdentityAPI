using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace IdentityAPI.ActionFilters
{
    public class ValidationFilterAttribute : IActionFilter
    {
        private readonly ILogger _logger;
        public ValidationFilterAttribute(ILogger<ValidationFilterAttribute> logger)
        {
            _logger = logger;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.RouteData.Values["action"];
            var controller = context.RouteData.Values["controller"];
            var param = context.ActionArguments
                .SingleOrDefault(x =>
                {
                    return x.Value.ToString()
                                  .Contains("Dto");
                }).Value;
            if (param == null)
            {
                _logger.LogError($"Object sent from client is null. Action: {action}, controller: {controller}");

                context.Result = new BadRequestObjectResult($"Object is null. action: {action}, controller: {controller}");
                return;
            }
            if (!context.ModelState.IsValid)
            {
                _logger.LogError($"Invalid model state for the object. action: {action}, controller: {controller}");
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
            }
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
