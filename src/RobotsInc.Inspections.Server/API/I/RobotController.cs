using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.BusinessLogic;
using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Server.Mappers;
using RobotsInc.Inspections.Server.Security;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.Server.API.I;

[ApiV1]
[Route(
    Inspections.API.I.Routes.ApiVersion
    + Inspections.API.I.Routes.Customers
    + "/" + CustomerController.RouteId
    + Inspections.API.I.Routes.Robots)]
public class RobotController
    : InspectionsController
{
    public const string IdentifierId = "robotId";
    public const string RouteId = "{" + IdentifierId + ":long:min(1)}";

    public RobotController(
        ILogger<RobotController> logger,
        ICustomerManager customerManager,
        IRobotManager<Robot> robotManager,
        IArticulatedRobotManager articulatedRobotManager,
        IAutomatedGuidedVehicleManager automatedGuidedVehicleManager,
        IArticulatedRobotMapper articulatedRobotMapper,
        IAutomatedGuidedVehicleMapper automatedGuidedVehicleMapper)
        : base(logger)
    {
        CustomerManager = customerManager;
        RobotManager = robotManager;
        ArticulatedRobotManager = articulatedRobotManager;
        AutomatedGuidedVehicleManager = automatedGuidedVehicleManager;
        ArticulatedRobotMapper = articulatedRobotMapper;
        AutomatedGuidedVehicleMapper = automatedGuidedVehicleMapper;
    }

    public ICustomerManager CustomerManager { get; }
    public IRobotManager<Robot> RobotManager { get; }
    public IArticulatedRobotManager ArticulatedRobotManager { get; }
    public IAutomatedGuidedVehicleManager AutomatedGuidedVehicleManager { get; }
    public IArticulatedRobotMapper ArticulatedRobotMapper { get; }
    public IAutomatedGuidedVehicleMapper AutomatedGuidedVehicleMapper { get; }

    /// <summary>
    ///     Create a new <see cref="Inspections.API.I.Robot" /> with the given properties.
    ///     Note that 2 types of robots are supported, each with their own characteristics: articulated robots and
    ///     automated guided vehicles.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robot">the robot to create</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Robot" />.
    /// </returns>
    /// <response code="201">
    ///     The given <paramref name="robot" /> is created successfully.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="robot" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    /// <response code="404">
    ///     The robot could not be created: the customer with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(Inspections.API.I.Robot), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateRobot(long customerId, Inspections.API.I.Robot robot, CancellationToken cancellationToken)
    {
        Customer? customer = await CustomerManager.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
        {
            return NotFound();
        }

        if (robot.Id != null)
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "Id", new[] { "must not be given" } }
                    });
            return BadRequest(problemDetails);
        }

        Inspections.API.I.Robot dto =
            robot switch
            {
                Inspections.API.I.ArticulatedRobot articulatedRobot =>
                    await CreateTypedRobot(articulatedRobot, customer, ArticulatedRobotMapper, ArticulatedRobotManager, cancellationToken),
                Inspections.API.I.AutomatedGuidedVehicle automatedGuidedVehicle =>
                    await CreateTypedRobot(automatedGuidedVehicle, customer, AutomatedGuidedVehicleMapper, AutomatedGuidedVehicleManager, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(robot), $"Type of robot ({robot.GetType().FullName}) is not supported.")
            };

        string? route = Url.Link(Routes.Robot_GetById, new { customerId, robotId = dto.Id });
        return Created(route!, dto);
    }

    private async Task<TDto> CreateTypedRobot<TDto, TModel>(
        TDto dto,
        Customer customer,
        IMapper<TModel, TDto> mapper,
        IManager<TModel> manager,
        CancellationToken cancellationToken)
        where TDto : Inspections.API.I.Robot, new()
        where TModel : Robot, new()
    {
        TModel model = await mapper.MapAsync(dto, cancellationToken);
        model.Customer = customer;
        await manager.SaveAsync(model, cancellationToken);
        return await mapper.MapAsync(model, cancellationToken);
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Robot" /> instances belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     A list of <see cref="Inspections.API.I.Robot" /> instances belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </returns>
    /// <response code="200">
    ///     A list of <see cref="Inspections.API.I.Robot" /> instances belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.  The list is
    ///     potentially empty.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Robot[]), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.CONSULT_INSPECTIONS)]
    public async Task<IActionResult> RetrieveRobots(long customerId, CancellationToken cancellationToken)
    {
        Customer? customer = await CustomerManager.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
        {
            return NotFound();
        }

        IList<Inspections.API.I.Robot> dto = new List<Inspections.API.I.Robot>();
        foreach (Robot robot in
                 customer
                     .Robots
                     .OrderByDescending(robot => robot.ManufacturingDate))
        {
            dto.Add(
                robot switch
                {
                    ArticulatedRobot articulatedRobot => await ArticulatedRobotMapper.MapAsync(articulatedRobot, cancellationToken),
                    AutomatedGuidedVehicle automatedGuidedVehicle => await AutomatedGuidedVehicleMapper.MapAsync(automatedGuidedVehicle, cancellationToken),
                    _ => throw new ArgumentOutOfRangeException(nameof(robot), $"Type of robot ({robot.GetType().FullName}) is not supported.")
                });
        }

        return Ok(dto.ToArray());
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to
    ///     the <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> was found and
    ///     returned.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpGet(RouteId, Name = Routes.Robot_GetById)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Robot), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.CONSULT_INSPECTIONS)]
    public async Task<IActionResult> RetrieveRobot(long customerId, long robotId, CancellationToken cancellationToken)
    {
        Robot? robot = await RobotManager.GetByIdAsync(robotId, customerId, cancellationToken);
        if (robot == null)
        {
            return NotFound();
        }

        Inspections.API.I.Robot dto =
            robot switch
            {
                ArticulatedRobot articulatedRobot => await ArticulatedRobotMapper.MapAsync(articulatedRobot, cancellationToken),
                AutomatedGuidedVehicle automatedGuidedVehicle => await AutomatedGuidedVehicleMapper.MapAsync(automatedGuidedVehicle, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(robot), $"Type of robot ({robot.GetType().FullName}) is not supported.")
            };

        return Ok(dto);
    }

    /// <summary>
    ///     Update the <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="robot">the updated properties of the robot</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" /> after the update.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> was found and
    ///     returned.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="robot" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpPut(RouteId)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Robot), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateRobot(long customerId, long robotId, Inspections.API.I.Robot robot, CancellationToken cancellationToken)
    {
        Robot? model = await RobotManager.GetByIdAsync(robotId, customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        if ((robot.Id != null) && (robot.Id != model.Id))
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "Id", new[] { "must not be given" } }
                    });
            return BadRequest(problemDetails);
        }

        if (model.RobotType != robot.RobotType)
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "RobotType", new[] { "Cannot change the type of a robot." } }
                    });
            return BadRequest(problemDetails);
        }

        Inspections.API.I.Robot dto = (robot, model) switch
        {
            (Inspections.API.I.ArticulatedRobot typedDto, ArticulatedRobot typedModel)
                => await UpdateTypedRobot(typedDto, typedModel, ArticulatedRobotMapper, ArticulatedRobotManager, cancellationToken),
            (Inspections.API.I.AutomatedGuidedVehicle typedModel, AutomatedGuidedVehicle typedDto)
                => await UpdateTypedRobot(typedModel, typedDto, AutomatedGuidedVehicleMapper, AutomatedGuidedVehicleManager, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(robot), $"Type of robot ({robot.GetType().FullName}) or model ({model.GetType().FullName}) is not supported.")
        };

        return Ok(dto);
    }

    private async Task<TDto> UpdateTypedRobot<TDto, TModel>(
        TDto dto,
        TModel model,
        IMapper<TModel, TDto> mapper,
        IManager<TModel> manager,
        CancellationToken cancellationToken)
        where TDto : Inspections.API.I.Robot, new()
        where TModel : Robot, new()
    {
        await mapper.MapAsync(dto, model, cancellationToken);
        await manager.SaveAsync(model, cancellationToken);
        return await mapper.MapAsync(model, cancellationToken);
    }

    /// <summary>
    ///     Delete the <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     Does not return a result.
    /// </returns>
    /// <response code="204">
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> was found and was
    ///     removed.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpDelete(RouteId)]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    public async Task<IActionResult> DeleteRobot(long customerId, long robotId, CancellationToken cancellationToken)
    {
        Robot? model = await RobotManager.GetByIdAsync(robotId, customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        await RobotManager.DeleteAsync(model, cancellationToken);
        return NoContent();
    }
}
