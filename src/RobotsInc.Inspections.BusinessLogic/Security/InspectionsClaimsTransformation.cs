using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

using Claim = RobotsInc.Inspections.Models.Security.Claim;
using ClaimTypes = RobotsInc.Inspections.API.I.Security.ClaimTypes;

namespace RobotsInc.Inspections.BusinessLogic.Security;

public class InspectionsClaimsTransformation : IClaimsTransformation
{
    public InspectionsClaimsTransformation(
        IHttpContextAccessor httpContextAccessor,
        IClaimManager claimManager)
    {
        HttpContextAccessor = httpContextAccessor;
        ClaimManager = claimManager;
    }

    public IHttpContextAccessor HttpContextAccessor { get; }
    public IClaimManager ClaimManager { get; }

    /// <inheritdoc />
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        string? userName = principal.Identity?.Name;
        bool enriched = principal.HasClaim(claim => claim.Type == ClaimTypes.Enriched);

        if (!string.IsNullOrWhiteSpace(userName) && !enriched)
        {
            CancellationToken cancellationToken =
                HttpContextAccessor.HttpContext?.RequestAborted
                ?? default;
            IList<Claim> claims = await ClaimManager.FindByUserEmailAsync(userName, cancellationToken);

            ClaimsIdentity claimsIdentity = new("Inspections", ClaimTypes.Email, ClaimTypes.Role);
            claimsIdentity.AddClaims(claims.Select(c => new System.Security.Claims.Claim(c.Type!, c.Value!)));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Enriched, "true"));
            principal.AddIdentity(claimsIdentity);
        }

        return principal;
    }
}
