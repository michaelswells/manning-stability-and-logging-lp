using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.API.I.Health;
using RobotsInc.Inspections.BusinessLogic.Health;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.Server.API.I;

[ApiV1]
[ApiV2]
[Route(
    Inspections.API.I.Routes.ApiVersion
    + Inspections.API.I.Routes.Health)]
[AllowAnonymous]
public class HealthController
    : InspectionsController
{
    public HealthController(
        ILogger<HealthController> logger,
        IHealthManager healthManager)
        : base(logger)
    {
        HealthManager = healthManager;
    }

    public IHealthManager HealthManager { get; }

    /// <summary>
    ///     A simple health check to verify that the backend is running as expected. The http status code of the
    ///     response is used to indicate whether the service can handle incoming requests at this moment.
    /// </summary>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>A <see cref="HealthResult" /> indicating the service health.</returns>
    /// <response code="200">
    ///     The service is considered healthy and can handle incoming requests.
    /// </response>
    /// <response code="503">
    ///     The service is not healthy and can currently not handle incoming requests.  The reason for this is on the
    ///     server side and the client should retry at a later time.
    /// </response>
    [HttpGet("")]
    [ProducesResponseType(typeof(HealthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResult), StatusCodes.Status503ServiceUnavailable)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(HealthResult), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status503ServiceUnavailable, null, typeof(HealthResult), ApplicationJson)]
    public async Task<IActionResult> Health(CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Requested health check.");
        HealthResult health = await HealthManager.CheckHealthAsync(cancellationToken);
        Logger.LogDebug("Health was {0}", health.Status);

        return
            health.Status == HealthStatus.HEALTHY
                ? Ok(health)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, health);
    }
}
