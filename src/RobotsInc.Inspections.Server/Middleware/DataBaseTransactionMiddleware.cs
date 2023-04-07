using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Server.Middleware;

public class DataBaseTransactionMiddleware : IMiddleware
{
    private readonly ILogger<DataBaseTransactionMiddleware> _logger = default!;
    private readonly InspectionsDbContext _inspectionsDbContext = default!;

    // private readonly RequestDelegate _requestDelegate = default!;

    public DataBaseTransactionMiddleware(
        ILogger<DataBaseTransactionMiddleware> logger,
        InspectionsDbContext inspectionsDbContext)
    {
        _logger = logger;
        _inspectionsDbContext = inspectionsDbContext;

        // _requestDelegate = requestDelegate;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<NoTransactionAttribute>() != null)
        {
            await next(context);
        }
        else
        {
            using (var transaction = await _inspectionsDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await next(context);

                    // This does not exist so I cannot check cancelation
                    /*if (transaction.GetDbTransaction().IsolationLevel == IsolationLevel.Chaos)
                    {
                        transaction.Rollback();
                    }
                    else*/
                    if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 400)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the request.");
                    transaction.Rollback();

                    // context.Response.StatusCode = StatusCodes.Status500InternalServerError;await
                    // context.Response.WriteAsync("An error occurred while processing the request.");
                    throw;
                }
            }
        }
    }
}
