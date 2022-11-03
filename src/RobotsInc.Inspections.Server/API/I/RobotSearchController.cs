using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.API.I;
using RobotsInc.Inspections.API.I.Security;
using RobotsInc.Inspections.BusinessLogic;
using RobotsInc.Inspections.Server.Mappers;
using RobotsInc.Inspections.Server.Security;

using Swashbuckle.AspNetCore.Annotations;

using ArticulatedRobot = RobotsInc.Inspections.Models.ArticulatedRobot;
using AutomatedGuidedVehicle = RobotsInc.Inspections.Models.AutomatedGuidedVehicle;
using Robot = RobotsInc.Inspections.Models.Robot;

namespace RobotsInc.Inspections.Server.API.I;

[ApiV1]
[Route(
    Inspections.API.I.Routes.ApiVersion
    + Inspections.API.I.Routes.Search
    + Inspections.API.I.Routes.Robots)]
public class RobotSearchController
    : InspectionsController
{
    public RobotSearchController(
        ILogger<RobotSearchController> logger,
        IRobotManager<Robot> robotManager,
        IMapper<ArticulatedRobot, Inspections.API.I.ArticulatedRobot> articulatedRobotMapper,
        IMapper<AutomatedGuidedVehicle, Inspections.API.I.AutomatedGuidedVehicle> automatedGuidedVehicleMapper)
        : base(logger)
    {
        RobotManager = robotManager;
        ArticulatedRobotMapper = articulatedRobotMapper;
        AutomatedGuidedVehicleMapper = automatedGuidedVehicleMapper;
    }

    public IRobotManager<Robot> RobotManager { get; }
    public IMapper<ArticulatedRobot, Inspections.API.I.ArticulatedRobot> ArticulatedRobotMapper { get; }
    public IMapper<AutomatedGuidedVehicle, Inspections.API.I.AutomatedGuidedVehicle> AutomatedGuidedVehicleMapper { get; }

    /// <summary>
    ///     Retrieve a paged list of robots that matches the given <paramref name="searchCriteria" />.
    /// </summary>
    /// <param name="searchCriteria">
    ///     the search criteria
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     A paged list of robots that match the given search criteria.
    /// </returns>
    /// <response code="200">
    ///     A paged list of robots that match the given search criteria, with metadata on the page and the total number
    ///     of matching robots.
    /// </response>
    /// <response code="400">
    ///     One or more parameters of the search request are missing or not valid.
    /// </response>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Robot[]), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.CONSULT_INSPECTIONS)]
    public async Task<IActionResult> SearchRobots(
        [FromQuery] RobotSearchCriteria searchCriteria,
        CancellationToken cancellationToken)
    {
        // authorization
        if (User.IsInRole(ClaimTypes.Values.RoleEmployee))
        {
            searchCriteria.CustomerIds = null;
        }
        else if (User.IsInRole(ClaimTypes.Values.RoleCustomer))
        {
            searchCriteria.CustomerIds =
                User.FindAll(ClaimTypes.Customer)
                    .Select(claim => Convert.ToInt64(claim.Value))
                    .ToArray();
        }

        PagedList<Robot> models =
            await RobotManager
                .FindByCriteriaAsync(searchCriteria, cancellationToken);

        IList<Inspections.API.I.Robot> items = new List<Inspections.API.I.Robot>();
        Debug.Assert(models.Items != null, "Items should not be null");
        foreach (Robot robot in models.Items)
        {
            items.Add(
                robot switch
                {
                    ArticulatedRobot articulatedRobot => await ArticulatedRobotMapper.MapAsync(articulatedRobot, cancellationToken),
                    AutomatedGuidedVehicle automatedGuidedVehicle => await AutomatedGuidedVehicleMapper.MapAsync(automatedGuidedVehicle, cancellationToken),
                    _ => throw new ArgumentOutOfRangeException(nameof(robot), $"Type of robot ({robot.GetType().FullName}) is not supported.")
                });
        }

        PagedList<Inspections.API.I.Robot> dtos =
            new()
            {
                Page = models.Page,
                PageSize = models.PageSize,
                TotalCount = models.TotalCount,
                TotalPages = models.TotalPages,
                Items = items.ToArray()
            };

        return Ok(dtos);
    }
}
