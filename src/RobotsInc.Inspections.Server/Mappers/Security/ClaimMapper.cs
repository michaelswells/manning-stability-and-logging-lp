using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using RobotsInc.Inspections.Models.Security;
using RobotsInc.Inspections.Repositories.Security;

namespace RobotsInc.Inspections.Server.Mappers.Security;

public class ClaimMapper : IMapper<Claim, Inspections.API.I.Security.Claim>
{
    private readonly IClaimRepository _claimRepository;

    public ClaimMapper(
        IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    /// <inheritdoc />
    public async Task<Inspections.API.I.Security.Claim> MapAsync(Claim model, CancellationToken cancellationToken)
    {
        Inspections.API.I.Security.Claim dto = new();
        await MapAsync(model, dto, cancellationToken);
        return dto;
    }

    /// <inheritdoc />
    public Task MapAsync(Claim model, Inspections.API.I.Security.Claim dto, CancellationToken cancellationToken)
    {
        dto.Id = model.Id;
        dto.Type = model.Type;
        dto.Value = model.Value;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Claim> MapAsync(Inspections.API.I.Security.Claim dto, CancellationToken cancellationToken)
    {
        Claim model;

        if (dto.Id != null)
        {
            Claim? claim = await _claimRepository.GetByIdAsync(dto.Id.Value, cancellationToken);
            model = claim ?? throw new BadHttpRequestException("Bad id");
        }
        else
        {
            model = new();
        }

        await MapAsync(dto, model, cancellationToken);
        return model;
    }

    /// <inheritdoc />
    public Task MapAsync(Inspections.API.I.Security.Claim dto, Claim model, CancellationToken cancellationToken)
    {
        model.Type = dto.Type;
        model.Value = dto.Value;
        return Task.CompletedTask;
    }
}
