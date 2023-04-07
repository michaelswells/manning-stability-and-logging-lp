using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.BusinessLogic;
using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Server.Filters;
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
    + Inspections.API.I.Routes.Notes
    + "/" + NoteController.RouteId
    + Inspections.API.I.Routes.Photos)]
public class PhotoController : InspectionsController
{
    public INoteManager NoteManager { get; }
    public IPhotoManager PhotoManager { get; }
    public const string IdentifierId = "photoId";
    public const string RouteId = "{" + IdentifierId + ":long:min(1)}";

    public PhotoController(
        ILogger<PhotoController> logger,
        INoteManager noteManager,
        IPhotoManager photoManager)
        : base(logger)
    {
        NoteManager = noteManager;
        PhotoManager = photoManager;
    }

    /// <summary>
    ///     Attach a new photo to the given note.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of a <see cref="Inspections.API.I.Inspection"/>
    /// </param>
    /// <param name="noteId">
    ///     the unique id of a <see cref="Inspections.API.I.Note"/>
    /// </param>
    /// <param name="file">
    ///     the photo as an uploaded file <see cref="IFormFile"/>
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Note" />.
    /// </returns>
    /// <response code="201">
    ///     The photo is created successfully.
    /// </response>
    /// <response code="400">
    ///     The request does not satisfy the validation criteria and the photo cannot be created.
    /// </response>
    /// <response code="404">
    ///     The photo could not be created: the note identified by the given <paramref name="noteId"/>,
    ///     <paramref name="inspectionId"/>, <paramref name="robotId"/> and <paramref name="customerId"/>
    ///     does not exist.
    /// </response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreatePhoto(
        long customerId,
        long robotId,
        long inspectionId,
        long noteId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        Note? note = await NoteManager.GetByIdAsync(noteId, inspectionId, robotId, customerId, cancellationToken);
        if (note == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(file.FileName)
            && !file.FileName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new InvalidPropertyException("Id", "must not be given");
        }

        byte[] content = new byte[file.Length];
        MemoryStream stream = new(content);
        await file.CopyToAsync(stream, cancellationToken);

        Photo model =
            new()
            {
                Note = note,
                Content = content
            };
        await PhotoManager.SaveAsync(model, cancellationToken);

        string? route =
            Url.Link(
                Routes.Photo_GetById,
                new { customerId, robotId, inspectionId, noteId, photoId = model.Id });
        return Created(route!, null);
    }

    /// <summary>
    ///     Retrieve references to all photos on a given note.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of a <see cref="Inspections.API.I.Inspection"/>
    /// </param>
    /// <param name="noteId">
    ///     the unique id of a <see cref="Inspections.API.I.Note"/>
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Note" />.
    /// </returns>
    /// <response code="200">
    ///     The photo is found and returned.
    /// </response>
    /// <response code="404">
    ///     The photo references could not be retrieved: the note identified by the given <paramref name="noteId"/>,
    ///     <paramref name="inspectionId"/>, <paramref name="robotId"/> and <paramref name="customerId"/> does not
    ///     exist.
    /// </response>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(string[]), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    public async Task<IActionResult> RetrievePhotos(
        long customerId,
        long robotId,
        long inspectionId,
        long noteId,
        CancellationToken cancellationToken)
    {
        Note? note = await NoteManager.GetByIdAsync(noteId, inspectionId, robotId, customerId, cancellationToken);
        if (note == null)
        {
            return NotFound();
        }

        Debug.Assert(note.Id != null, "Id should not be null, coming from database.");
        long[] photoIds = await PhotoManager.FindIdsByNoteIdAsync(note.Id.Value, cancellationToken);
        string[] dto =
            photoIds
                .Select(
                    photoId =>
                    {
                        string? route =
                            Url.Link(
                                Routes.Photo_GetById,
                                new { customerId, robotId, inspectionId, noteId, photoId });
                        Debug.Assert(route != null, "Route should always exist.");
                        return route;
                    })
                .ToArray();
        return Ok(dto);
    }

    /// <summary>
    ///     Retrieve a specific photo from a given note.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of a <see cref="Inspections.API.I.Inspection"/>
    /// </param>
    /// <param name="noteId">
    ///     the unique id of a <see cref="Inspections.API.I.Note"/>
    /// </param>
    /// <param name="photoId">
    ///     the unique id of a photo
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Note" />.
    /// </returns>
    /// <response code="200">
    ///     The photo is found and returned.
    /// </response>
    /// <response code="404">
    ///     The photo could not be retrieved: the photo identified by the given <paramref name="photoId"/>,
    ///     <paramref name="noteId"/>, <paramref name="inspectionId"/>, <paramref name="robotId"/> and
    ///     <paramref name="customerId"/> does not exist.
    /// </response>
    [HttpGet(RouteId, Name = Routes.Photo_GetById)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(void), ApplicationOctetStream)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    public async Task<IActionResult> RetrievePhoto(
        long customerId,
        long robotId,
        long inspectionId,
        long noteId,
        long photoId,
        CancellationToken cancellationToken)
    {
        Photo? photo = await PhotoManager.GetByIdAsync(photoId, noteId, inspectionId, robotId, customerId, cancellationToken);
        if (photo == null)
        {
            return NotFound();
        }

        Debug.Assert(photo.Content != null, "Content should not be null, coming from the database.");
        return new FileContentResult(photo.Content!, "image/jpg");
    }

    /// <summary>
    ///     Delete a specific photo from a given note.
    /// </summary>
    /// <param name="customerId">
    ///     the unique id of the <see cref="Inspections.API.I.Customer" /> that owns the robot
    /// </param>
    /// <param name="robotId">
    ///     the unique id of a <see cref="Inspections.API.I.Robot" />
    /// </param>
    /// <param name="inspectionId">
    ///     the unique id of a <see cref="Inspections.API.I.Inspection"/>
    /// </param>
    /// <param name="noteId">
    ///     the unique id of a <see cref="Inspections.API.I.Note"/>
    /// </param>
    /// <param name="photoId">
    ///     the unique id of a photo
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Note" />.
    /// </returns>
    /// <response code="204">
    ///     The photo is found and removed.
    /// </response>
    /// <response code="404">
    ///     The photo could not be found: the photo identified by the given <paramref name="photoId"/>,
    ///     <paramref name="noteId"/>, <paramref name="inspectionId"/>, <paramref name="robotId"/> and
    ///     <paramref name="customerId"/> does not exist.
    /// </response>
    [HttpDelete(RouteId)]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    public async Task<IActionResult> DeletePhoto(
        long customerId,
        long robotId,
        long inspectionId,
        long noteId,
        long photoId,
        CancellationToken cancellationToken)
    {
        Photo? photo = await PhotoManager.GetByIdAsync(photoId, noteId, inspectionId, robotId, customerId, cancellationToken);
        if (photo == null)
        {
            return NotFound();
        }

        await PhotoManager.DeleteAsync(photo, cancellationToken);

        return NoContent();
    }
}
