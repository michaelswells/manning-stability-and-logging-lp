using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models.Security;

namespace RobotsInc.Inspections.BusinessLogic.Security;

public interface IClaimManager : IManager<Claim>
{
    Task<Claim?> GetByIdAsync(long claimId, long userId, CancellationToken cancellationToken);

    Task<IList<Claim>> FindByUserEmailAsync(string email, CancellationToken cancellationToken);
}
