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
    + Inspections.API.I.Routes.Inspections
    + "/" + InspectionController.RouteId
    + Inspections.API.I.Routes.Notes)]
public class NoteController
    : InspectionsController
{
    public const string IdentifierId = "noteId";
    public const string RouteId = "{" + IdentifierId + ":long:min(1)}";

    public NoteController(
        ILogger<NoteController> logger,
        IMapper<Note, Inspections.API.I.Note> noteMapper,
        IInspectionManager inspectionManager,
        INoteManager noteManager)
        : base(logger)
    {
        NoteMapper = noteMapper;
        InspectionManager = inspectionManager;
        NoteManager = noteManager;
    }

    public IMapper<Note, Inspections.API.I.Note> NoteMapper { get; }
    public IInspectionManager InspectionManager { get; }
    public INoteManager NoteManager { get; }

    /// <summary>
    ///     Create a new <see cref="Inspections.API.I.Note" /> with the given properties.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of a <see cref="Inspections.API.I.Note"/>
    /// </param>
    /// <param name="note">
    ///     the note to create
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Note" />.
    /// </returns>
    /// <response code="201">
    ///     The given <paramref name="note" /> is created successfully.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="note" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    /// <response code="404">
    ///     The note could not be created: the inspection with the given <paramref name="inspectionId"/> for the
    ///     robot with the given <paramref name="robotId"/>, belonging to the
    ///     customer with the given <paramref name="customerId" /> does not exist.
    /// </response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(Inspections.API.I.Inspection), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateNote(
        long customerId,
        long robotId,
        long inspectionId,
        Inspections.API.I.Note note,
        CancellationToken cancellationToken)
    {
        Inspection? inspection = await InspectionManager.GetByIdAsync(inspectionId, robotId, customerId, cancellationToken);
        if (inspection == null)
        {
            return NotFound();
        }

        if (note.Id != null)
        {
            throw new InvalidPropertyException("Id", "must not be given");
        }

        Note model = await NoteMapper.MapAsync(note, cancellationToken);
        model.Inspection = inspection;
        await NoteManager.SaveAsync(model, cancellationToken);
        Inspections.API.I.Note dto = await NoteMapper.MapAsync(model, cancellationToken);

        string? route =
            Url.Link(
                Routes.Note_GetById,
                new { customerId, robotId, inspectionId, noteId = dto.Id });
        return Created(route!, dto);
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Note"/> instances on the
    ///     <see cref="Inspections.API.I.Inspection" /> with the given <paramref name="inspectionId" />,
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
    ///     A list of <see cref="Inspections.API.I.Note"/> instances for the <see cref="Inspections.API.I.Inspection" />
    ///     with the given <paramref name="inspectionId" />.
    /// </returns>
    /// <response code="200">
    ///     A list of <see cref="Inspections.API.I.Note"/> instances for the <see cref="Inspections.API.I.Inspection" />
    ///     with the given <paramref name="inspectionId" />.  The list is potentially empty.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />, does not exist.
    /// </response>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Note[]), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.CONSULT_INSPECTIONS)]
    public async Task<IActionResult> RetrieveNotes(
        long customerId,
        long robotId,
        long inspectionId,
        CancellationToken cancellationToken)
    {
        Inspection? inspection = await InspectionManager.GetByIdAsync(inspectionId, robotId, customerId, cancellationToken);
        if (inspection == null)
        {
            return NotFound();
        }

        IList<Inspections.API.I.Note> dto = new List<Inspections.API.I.Note>();
        foreach (Note note in
                 inspection
                     .Notes
                     .OrderByDescending(note => note.Importance))
        {
            dto.Add(await NoteMapper.MapAsync(note, cancellationToken));
        }

        return Ok(dto.ToArray());
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> on the
    ///     <see cref="Inspections.API.I.Inspection" /> with the given <paramref name="inspectionId" />,
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
    /// <param name="noteId">
    ///     the unique id of an <see cref="Inspections.API.I.Note" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/>.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> was found and is
    ///     returned.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> on the
    ///     <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />, does not exist.
    /// </response>
    [HttpGet(RouteId, Name = Routes.Note_GetById)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Note), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.CONSULT_INSPECTIONS)]
    public async Task<IActionResult> RetrieveNote(
        long customerId,
        long robotId,
        long inspectionId,
        long noteId,
        CancellationToken cancellationToken)
    {
        Note? note =
            await NoteManager.GetByIdAsync(noteId, inspectionId, robotId, customerId, cancellationToken);
        if (note == null)
        {
            return NotFound();
        }

        Inspections.API.I.Note dto = await NoteMapper.MapAsync(note, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Update the <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> on
    ///     the <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for
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
    /// <param name="noteId">
    ///     the unique id of an <see cref="Inspections.API.I.Note" />
    /// </param>
    /// <param name="note">
    ///     the updated properties of the note
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Note" /> with the given <paramref name="noteId" /> after the
    ///     update.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> was found and upated,
    ///     and is returned.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="note" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> on the
    ///     <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />, does not exist.
    /// </response>
    [HttpPut(RouteId)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Note), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateNote(
        long customerId,
        long robotId,
        long inspectionId,
        long noteId,
        Inspections.API.I.Note note,
        CancellationToken cancellationToken)
    {
        Note? model =
            await NoteManager.GetByIdAsync(noteId, inspectionId, robotId, customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        if ((note.Id != null) && (note.Id != model.Id))
        {
            throw new InvalidPropertyException("Id", "must not be given");
        }

        await NoteMapper.MapAsync(note, model, cancellationToken);
        await NoteManager.SaveAsync(model, cancellationToken);
        Inspections.API.I.Note dto = await NoteMapper.MapAsync(model, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Delete the <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> on the
    ///     <see cref="Inspections.API.I.Inspection" /> with the given <paramref name="inspectionId" />,
    ///     for the <see cref="Inspections.API.I.Robot"/> with the given <paramref name="robotId"/>, belonging to
    ///     the <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" />
    /// </param>
    /// <param name="robotId">
    ///     the unique id of the <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of an <see cref="Inspections.API.I.Inspection" />
    /// </param>
    /// <param name="noteId">
    ///     the unique id of an <see cref="Inspections.API.I.Note" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     Does not return a result.
    /// </returns>
    /// <response code="204">
    ///     The <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> was found and is
    ///     removed.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Note"/> with the given <paramref name="noteId"/> on the
    ///     <see cref="Inspections.API.I.Inspection"/> with the given <paramref name="inspectionId"/> for the
    ///     <see cref="Inspections.API.I.Robot" /> with the given <paramref name="robotId" />, belonging to the
    ///     <see cref="Inspections.API.I.Customer" /> with the given <paramref name="customerId" />, does not exist.
    /// </response>
    [HttpDelete(RouteId)]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    public async Task<IActionResult> DeleteNote(
        long customerId,
        long robotId,
        long inspectionId,
        long noteId,
        CancellationToken cancellationToken)
    {
        Note? model =
            await NoteManager.GetByIdAsync(noteId, inspectionId, robotId, customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        await NoteManager.DeleteAsync(model, cancellationToken);
        return NoContent();
    }
}
