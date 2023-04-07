using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RobotsInc.Inspections.Server.Filters;

public class InvalidPropertyExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<InvalidPropertyExceptionFilter> _logger = default!;

    public InvalidPropertyExceptionFilter(ILogger<InvalidPropertyExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is InvalidPropertyException invalidPropertyException)
        {
            _logger.LogDebug("logging an InvalidPropertyException.");
            var problemDetails = new HttpValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    { invalidPropertyException.PropertyName, new[] { invalidPropertyException.Message } }
                });

            context.Result = new BadRequestObjectResult(problemDetails);
            context.ExceptionHandled = true;
        }

        await Task.CompletedTask;
    }
}
