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
    + Inspections.API.I.Routes.Users
    + "/" + UserController.RouteId
    + Inspections.API.I.Routes.Claims)]
[InspectionsAuthorize(Policy.MANAGE_USER_CLAIMS)]
public class ClaimController : InspectionsController
{
    public const string IdentifierId = "claimId";
    public const string RouteId = "{" + IdentifierId + ":long:min(1)}";

    public ClaimController(
        ILogger<ClaimController> logger,
        IMapper<Claim, Inspections.API.I.Security.Claim> claimMapper,
        IUserManager userManager,
        IClaimManager claimManager)
        : base(logger)
    {
        ClaimMapper = claimMapper;
        UserManager = userManager;
        ClaimManager = claimManager;
    }

    public IMapper<Claim, Inspections.API.I.Security.Claim> ClaimMapper { get; }
    public IUserManager UserManager { get; }
    public IClaimManager ClaimManager { get; }

    /// <summary>
    ///     Create a new <see cref="Inspections.API.I.Security.Claim" /> with the given properties.
    /// </summary>
    /// <param name="userId">
    ///     the unique id of the <see cref="Inspections.API.I.Security.User" /> that owns the claim
    /// </param>
    /// <param name="claim">
    ///     the claim to create
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Security.Claim" />.
    /// </returns>
    /// <response code="201">
    ///     The given <paramref name="claim" /> is created successfully.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="claim" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    /// <response code="404">
    ///     The claim could not be created: the user with the given <paramref name="userId"/> does not exist.
    /// </response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(Inspections.API.I.Security.Claim), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateClaim(
        long userId,
        Inspections.API.I.Security.Claim claim,
        CancellationToken cancellationToken)
    {
        User? user = await UserManager.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        if (claim.Id != null)
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "Id", new[] { "must not be given" } }
                    });
            return BadRequest(problemDetails);
        }

        Claim model = await ClaimMapper.MapAsync(claim, cancellationToken);
        model.User = user;
        await ClaimManager.SaveAsync(model, cancellationToken);
        Inspections.API.I.Security.Claim dto = await ClaimMapper.MapAsync(model, cancellationToken);

        string? route =
            Url.Link(
                Routes.Claim_GetById,
                new { userId, claimId = dto.Id });
        return Created(route!, dto);
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/> on the
    ///     <see cref="Inspections.API.I.Security.User" /> with the given <paramref name="userId" />.
    /// </summary>
    /// <param name="userId">
    ///     the unique id of the <see cref="Inspections.API.I.Security.User" /> that owns the robot for which
    /// </param>
    /// <param name="claimId">
    ///     the unique id of an <see cref="Inspections.API.I.Security.Claim" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/>.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/> was found and is
    ///     returned.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/>, belonging to
    ///     the <see cref="Inspections.API.I.Security.User" /> with the given <paramref name="userId" />, does not exist.
    /// </response>
    [HttpGet(RouteId, Name = Routes.Claim_GetById)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Security.Claim), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    public async Task<IActionResult> RetrieveClaim(
        long userId,
        long claimId,
        CancellationToken cancellationToken)
    {
        Claim? claim = await ClaimManager.GetByIdAsync(claimId, userId, cancellationToken);
        if (claim == null)
        {
            return NotFound();
        }

        Inspections.API.I.Security.Claim dto = await ClaimMapper.MapAsync(claim, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Update the <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/>
    ///     belonging to the <see cref="Inspections.API.I.Security.User" /> with the given <paramref name="userId" />.
    /// </summary>
    /// <param name="userId">
    ///     the unique id of the <see cref="Inspections.API.I.Security.User" /> that owns the claim
    /// </param>
    /// <param name="claimId">
    ///     the unique id of an <see cref="Inspections.API.I.Security.Claim" />
    /// </param>
    /// <param name="claim">
    ///     the updated properties of the claim
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Security.Claim" /> with the given <paramref name="claimId" /> after the
    ///     update.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/> was found and
    ///     updated, and is returned.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="claim" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/> belonging to the
    ///     <see cref="Inspections.API.I.Security.User" /> with the given <paramref name="userId" />, does not exist.
    /// </response>
    [HttpPut(RouteId)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Security.Claim), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateClaim(
        long userId,
        long claimId,
        Inspections.API.I.Security.Claim claim,
        CancellationToken cancellationToken)
    {
        Claim? model =
            await ClaimManager.GetByIdAsync(claimId, userId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        if ((claim.Id != null) && (claim.Id != model.Id))
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "Id", new[] { "must not be given" } }
                    });
            return BadRequest(problemDetails);
        }

        await ClaimMapper.MapAsync(claim, model, cancellationToken);
        await ClaimManager.SaveAsync(model, cancellationToken);
        Inspections.API.I.Security.Claim dto = await ClaimMapper.MapAsync(model, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Delete the <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/> belonging
    ///     to the <see cref="Inspections.API.I.Security.User" /> with the given <paramref name="userId" />.
    /// </summary>
    /// <param name="userId">
    ///     the unique id of the <see cref="Inspections.API.I.Security.User" />
    /// </param>
    /// <param name="claimId">
    ///     the unique id of an <see cref="Inspections.API.I.Security.Claim" />
    /// </param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     Does not return a result.
    /// </returns>
    /// <response code="204">
    ///     The <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/> was found and is
    ///     removed.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Security.Claim"/> with the given <paramref name="claimId"/> belonging to the
    ///     <see cref="Inspections.API.I.Security.User" /> with the given <paramref name="userId" />, does not exist.
    /// </response>
    [HttpDelete(RouteId)]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    public async Task<IActionResult> DeleteClaim(
        long userId,
        long claimId,
        CancellationToken cancellationToken)
    {
        Claim? model =
            await ClaimManager.GetByIdAsync(claimId, userId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        await ClaimManager.DeleteAsync(model, cancellationToken);
        return NoContent();
    }
}
