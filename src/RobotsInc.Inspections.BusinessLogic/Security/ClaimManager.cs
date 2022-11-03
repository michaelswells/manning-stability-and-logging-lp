using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Storage;

using RobotsInc.Inspections.Models.Security;
using RobotsInc.Inspections.Repositories;
using RobotsInc.Inspections.Repositories.Security;

namespace RobotsInc.Inspections.BusinessLogic.Security;

public class ClaimManager
    : Manager<Claim>,
      IClaimManager
{
    public ClaimManager(
        InspectionsDbContext inspectionsDbContext,
        IClaimRepository claimRepository)
        : base(inspectionsDbContext, claimRepository)
    {
        ClaimRepository = claimRepository;
    }

    public IClaimRepository ClaimRepository { get; }

    /// <inheritdoc />
    public async Task<Claim?> GetByIdAsync(long claimId, long userId, CancellationToken cancellationToken)
    {
        Claim? claim = await GetByIdAsync(claimId, cancellationToken);
        return
            claim?.User?.Id == userId
                ? claim
                : null;
    }

    /// <inheritdoc />
    public async Task<IList<Claim>> FindByUserEmailAsync(string email, CancellationToken cancellationToken)
    {
        IDbContextTransaction transaction = await InspectionsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            IList<Claim> claims = await ClaimRepository.FindByUserEmailAsync(email, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return claims;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
