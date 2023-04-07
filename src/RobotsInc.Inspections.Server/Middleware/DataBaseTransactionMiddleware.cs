using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Server.Middleware;

public class DataBaseTransactionMiddleware : IMiddleware
{
    private readonly ILogger<DataBaseTransactionMiddleware> _logger = default!;
    private readonly InspectionsDbContext _inspectionsDbContext = default!;

    private readonly RequestDelegate _requestDelegate = default!;

    public DataBaseTransactionMiddleware(
        ILogger<DataBaseTransactionMiddleware> logger,
        InspectionsDbContext inspectionsDbContext,
        RequestDelegate requestDelegate)
    {
        _logger = logger;
        _inspectionsDbContext = inspectionsDbContext;
        _requestDelegate = requestDelegate;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        int statusCode = context.Response.StatusCode;
        CancellationToken cancellationToken = CancellationToken.None;

        using (var transaction = await _inspectionsDbContext.Database.BeginTransactionAsync())
        {
            try
            {
                await next(context);

                if (transaction.GetDbTransaction().IsolationLevel == IsolationLevel.Chaos)
                {
                    transaction.Rollback();
                }
                else if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 400)
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
            }
        }

        await next(context);
    }
}
