using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models.Security;

namespace RobotsInc.Inspections.Repositories.Security;

public interface IClaimRepository : IRepository<Claim>
{
    Task<IList<Claim>> FindByUserEmailAsync(string email, CancellationToken cancellationToken);
}
