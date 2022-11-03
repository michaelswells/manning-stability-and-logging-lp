using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.BusinessLogic.Security;
using RobotsInc.Inspections.Models.Security;
using RobotsInc.Inspections.Server.Mappers;
using RobotsInc.Inspections.Server.Security;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.Server.API.I;

[ApiV1]
[Route(
    Inspections.API.I.Routes.ApiVersion
    + Inspections.API.I.Routes.Users)]
[InspectionsAuthorize(Policy.MANAGE_USER_CLAIMS)]
public class UserController : InspectionsController
{
    public const string IdentifierId = "userId";
    public const string RouteId = "{" + IdentifierId + ":long:min(1)}";

    public UserController(
        ILogger<UserController> logger,
        IUserManager userManager,
        IMapper<User, Inspections.API.I.Security.User> userMapper)
        : base(logger)
    {
        UserManager = userManager;
        UserMapper = userMapper;
    }

    public IUserManager UserManager { get; }
    public IMapper<User, Inspections.API.I.Security.User> UserMapper { get; }

    /// <summary>
    ///     Create a new <see cref="RobotsInc.Inspections.API.I.Security.User" /> with the given properties.
    /// </summary>
    /// <param name="user">the user to create</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="RobotsInc.Inspections.API.I.Security.User" />.
    /// </returns>
    /// <response code="201">
    ///     The given <paramref name="user" /> is created successfully.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="user" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(Inspections.API.I.Security.User), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateUser(Inspections.API.I.Security.User user, CancellationToken cancellationToken)
    {
        if (user.Id != null)
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "Id", new[] { "must not be given" } }
                    });
            return BadRequest(problemDetails);
        }

        User model = await UserMapper.MapAsync(user, cancellationToken);
        await UserManager.SaveAsync(model, cancellationToken);
        Inspections.API.I.Security.User dto = await UserMapper.MapAsync(model, cancellationToken);

        string? route =
            Url.Link(
                Routes.User_GetById,
                new { userId = dto.Id });
        return Created(route!, dto);
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">the unique id of a user</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/>.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/> was found and
    ///     is returned.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/> does not exist.
    /// </response>
    [HttpGet(RouteId, Name = Routes.User_GetById)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Security.User), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    public async Task<IActionResult> RetrieveUser(long userId, CancellationToken cancellationToken)
    {
        User? model = await UserManager.GetByIdAsync(userId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        Inspections.API.I.Security.User dto = await UserMapper.MapAsync(model, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Update the <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">the unique id of a user</param>
    /// <param name="user">the updated properties for the user</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/> after the update.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/> was found, was
    ///     updated and the updated <see cref="Inspections.API.I.Security.User"/> is returned.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="user"/> properties do not pass the validation requirements.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/> does not exist.
    /// </response>
    [HttpPut(RouteId)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Security.User), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateUser(long userId, Inspections.API.I.Security.User user, CancellationToken cancellationToken)
    {
        User? model = await UserManager.GetByIdAsync(userId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        if ((user.Id != null) && (user.Id != model.Id))
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "Id", new[] { "must not be given" } }
                    });
            return BadRequest(problemDetails);
        }

        await UserMapper.MapAsync(user, model, cancellationToken);
        await UserManager.SaveAsync(model, cancellationToken);

        Inspections.API.I.Security.User dto = await UserMapper.MapAsync(model, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Delete the <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">the unique id of a user</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     Does not return a result.
    /// </returns>
    /// <response code="204">
    ///     The <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/> was found and was
    ///     removed.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Security.User"/> with the given <paramref name="userId"/> does not exist.
    /// </response>
    [HttpDelete(RouteId)]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    public async Task<IActionResult> DeleteUser(long userId, CancellationToken cancellationToken)
    {
        User? model = await UserManager.GetByIdAsync(userId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        await UserManager.DeleteAsync(model, cancellationToken);
        return NoContent();
    }
}
