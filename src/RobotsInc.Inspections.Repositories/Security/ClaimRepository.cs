using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using RobotsInc.Inspections.Models.Security;

namespace RobotsInc.Inspections.Repositories.Security;

public class ClaimRepository
    : Repository<Claim>,
      IClaimRepository
{
    public ClaimRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }

    /// <inheritdoc />
    public async Task<IList<Claim>> FindByUserEmailAsync(string email, CancellationToken cancellationToken)
    {
        IList<Claim> claims =
            await InspectionsDbContext
                .Claims
                .Where(claim => claim.User!.Email == email)
                .ToListAsync(cancellationToken);
        return claims;
    }
}
