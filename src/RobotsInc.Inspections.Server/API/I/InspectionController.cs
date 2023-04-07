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
using RobotsInc.Inspections.Server.Filters;
using RobotsInc.Inspections.Server.Mappers;
using RobotsInc.Inspections.Server.Security;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.Server.API.I;

[ApiV1]
[Route(
    Inspections.API.I.Routes.ApiVersion
    + Inspections.API.I.Routes.Customers
    + "/" + CustomerController.RouteId
    + Inspections.API.I.Routes.Robots
    + "/" + RobotController.RouteId
    + Inspections.API.I.Routes.Inspections)]
public class InspectionController
    : InspectionsController
{
    public const string IdentifierId = "inspectionId";
    public const string RouteId = "{" + IdentifierId + ":long:min(1)}";

    public InspectionController(
        ILogger<InspectionController> logger,
        IMapper<Inspection, Inspections.API.I.Inspection> inspectionMapper,
        IRobotManager<Robot> robotManager,
        IInspectionManager inspectionManager)
        : base(logger)
    {
        InspectionMapper = inspectionMapper;
        RobotManager = robotManager;
        InspectionManager = inspectionManager;
    }

    public IMapper<Inspection, Inspections.API.I.Inspection> InspectionMapper { get; }
    public IRobotManager<Robot> RobotManager { get; }
    public IInspectionManager InspectionManager { get; }

    /// <summary>
    ///     Create a new <see cref="Inspections.API.I.Inspection" /> with the given properties.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspection">the inspection to create for the robot</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Inspection" />.
    /// </returns>
    /// <response code="201">
    ///     The given <paramref name="inspection" /> is created successfully.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="inspection" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    /// <response code="404">
    ///     The inspection could not be created: the robot with the given <paramref name="robotId"/>, belonging to the
    ///     customer with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(Inspections.API.I.Inspection), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateInspection(
        long customerId,
        long robotId,
        Inspections.API.I.Inspection inspection,
        CancellationToken cancellationToken)
    {
        Robot? robot = await RobotManager.GetByIdAsync(robotId, customerId, cancellationToken);
        if (robot == null)
        {
            return NotFound();
        }

        if (inspection.Id != null)
        {
            throw new InvalidPropertyException("Id", "must not be given");
        }

        Inspection model = await InspectionMapper.MapAsync(inspection, cancellationToken);
        model.Robot = robot;
        await InspectionManager.SaveAsync(model, cancellationToken);
        Inspections.API.I.Inspection dto = await InspectionMapper.MapAsync(model, cancellationToken);

        string? route =
            Url.Link(
                Routes.Inspection_GetById,
                new { customerId, robotId, inspectionId = dto.Id });
        return Created(route!, dto);
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Inspection" /> instances for the
    ///     <see cref="Inspections.API.I.Robot"/> with the given <paramref name="robotId"/>, belonging to
    ///     the <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot for which the
    ///     <see cref="Inspections.API.I.Inspection" /> instances are retrieved
    /// </param>
    /// <param name="robotId">
    ///     the unique id of the <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     A list of <see cref="Inspections.API.I.Inspection"/> instances for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </returns>
    /// <response code="200">
    ///     A list of <see cref="Inspections.API.I.Inspection"/> instances for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> is returned.
    ///     The list is potentially empty.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />, does not exist.
    /// </response>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Inspection[]), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.CONSULT_INSPECTIONS)]
    public async Task<IActionResult> RetrieveInspections(
        long customerId,
        long robotId,
        CancellationToken cancellationToken)
    {
        Robot? robot = await RobotManager.GetByIdAsync(robotId, customerId, cancellationToken);
        if (robot == null)
        {
            return NotFound();
        }

        IList<Inspections.API.I.Inspection> dto = new List<Inspections.API.I.Inspection>();
        foreach (Inspection inspection in
                 robot
                     .Inspections
                     .OrderByDescending(inspection => inspection.Date))
        {
            dto.Add(await InspectionMapper.MapAsync(inspection, cancellationToken));
        }

        return Ok(dto.ToArray());
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Inspection" /> with the given <paramref name="inspectionId" />,
    ///     for the <see cref="Inspections.API.I.Robot"/> with the given <paramref name="robotId"/>, belonging to
    ///     the <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot for which
    /// </param>
    /// <param name="robotId">
    ///     the unique id of the <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of an <see cref="Inspections.API.I.Inspection" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />, was found and
    ///     returned.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />, does not exist.
    /// </response>
    [HttpGet(RouteId, Name = Routes.Inspection_GetById)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Inspection), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.CONSULT_INSPECTIONS)]
    public async Task<IActionResult> RetrieveInspection(
        long customerId,
        long robotId,
        long inspectionId,
        CancellationToken cancellationToken)
    {
        Inspection? inspection =
            await InspectionManager.GetByIdAsync(inspectionId, robotId, customerId, cancellationToken);
        if (inspection == null)
        {
            return NotFound();
        }

        foreach (Note note in inspection.Notes)
        {
            Console.WriteLine($"Note: {note.Summary}");
        }

        Inspections.API.I.Inspection dto = await InspectionMapper.MapAsync(inspection, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Update the <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for
    ///     the <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot for which
    /// </param>
    /// <param name="robotId">
    ///     the unique id of the <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of an <see cref="Inspections.API.I.Inspection" />
    /// </param>
    /// <param name="inspection">
    ///     the updated properties of the inspection
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Inspection" /> with the given <paramref name="inspectionId" /> after the
    ///     update.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for
    ///     the <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> was found and
    ///     returned.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="inspection" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for
    ///     the <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpPut(RouteId)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Inspection), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateInspection(
        long customerId,
        long robotId,
        long inspectionId,
        Inspections.API.I.Inspection inspection,
        CancellationToken cancellationToken)
    {
        Inspection? model =
            await InspectionManager.GetByIdAsync(inspectionId, robotId, customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        if ((inspection.Id != null) && (inspection.Id != model.Id))
        {
            throw new InvalidPropertyException("Id", "must not be given");
        }

        await InspectionMapper.MapAsync(inspection, model, cancellationToken);
        await InspectionManager.SaveAsync(model, cancellationToken);
        Inspections.API.I.Inspection dto = await InspectionMapper.MapAsync(model, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Delete the <see cref="RobotsInc.Inspections.API.I.Inspection"/> with the given
    ///     <paramref name="inspectionId"/> for the <see cref="Inspections.API.I.Robot" /> with the given
    ///     <paramref name="robotId" />, belonging to the <see cref="Inspections.API.I.Customer" /> with the given
    ///     <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" />
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of an <see cref="Inspections.API.I.Inspection" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     Does not return a result.
    /// </returns>
    /// <response code="204">
    ///     The <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> was found and was
    ///     removed.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for
    ///     the <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpDelete(RouteId)]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    public async Task<IActionResult> DeleteInspection(
        long customerId,
        long robotId,
        long inspectionId,
        CancellationToken cancellationToken)
    {
        Inspection? model =
            await InspectionManager.GetByIdAsync(inspectionId, robotId, customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        await InspectionManager.DeleteAsync(model, cancellationToken);
        return NoContent();
    }
}
