using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.Server.Security;

namespace RobotsInc.Inspections.Server.API;

[ApiController]
[InspectionsAuthorize]
[EnableCors("UI")]
public abstract class InspectionsController : ControllerBase
{
    protected const string ApplicationJson = "application/json";
    protected const string ApplicationProblemJson = "application/problem+json";
    protected const string ApplicationOctetStream = "application/octet-stream";

    protected InspectionsController(ILogger logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }
}
