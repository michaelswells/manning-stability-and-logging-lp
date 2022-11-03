using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;

using Claim = RobotsInc.Inspections.Models.Security.Claim;
using ClaimTypes = RobotsInc.Inspections.API.I.Security.ClaimTypes;

namespace RobotsInc.Inspections.BusinessLogic.Security;

public class InspectionsClaimsTransformation : IClaimsTransformation
{
    public InspectionsClaimsTransformation(
        IClaimManager claimManager)
    {
        ClaimManager = claimManager;
    }

    public IClaimManager ClaimManager { get; }

    /// <inheritdoc />
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        string? userName = principal.Identity?.Name;
        bool enriched = principal.HasClaim(claim => claim.Type == ClaimTypes.Enriched);

        if (!string.IsNullOrWhiteSpace(userName) && !enriched)
        {
            ClaimsIdentity claimsIdentity = new(null, ClaimTypes.Email, ClaimTypes.Role);
            IList<Claim> claims = await ClaimManager.FindByUserEmailAsync(userName, default);
            claimsIdentity.AddClaims(claims.Select(c => new System.Security.Claims.Claim(c.Type!, c.Value!)));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Enriched, "true"));

            principal.AddIdentity(claimsIdentity);
        }

        return principal;
    }
}
