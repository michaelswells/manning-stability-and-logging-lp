using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Server.Mappers;

public class InspectionMapper : IInspectionMapper
{
    private readonly IInspectionRepository _inspectionRepository;

    public InspectionMapper(
        IInspectionRepository inspectionRepository)
    {
        _inspectionRepository = inspectionRepository;
    }

    /// <inheritdoc />
    public async Task<Inspections.API.I.Inspection> MapAsync(Inspection model, CancellationToken cancellationToken)
    {
        Inspections.API.I.Inspection dto = new();
        await MapAsync(model, dto, cancellationToken);
        return dto;
    }

    /// <inheritdoc />
    public Task MapAsync(Inspection model, Inspections.API.I.Inspection dto, CancellationToken cancellationToken)
    {
        dto.Id = model.Id;
        dto.Date = model.Date;
        dto.State = model.State;
        dto.Summary = model.Summary;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Inspection> MapAsync(Inspections.API.I.Inspection dto, CancellationToken cancellationToken)
    {
        Inspection model;

        if (dto.Id != null)
        {
            Inspection? inspection = await _inspectionRepository.GetByIdAsync(dto.Id.Value, cancellationToken);
            model = inspection ?? throw new BadHttpRequestException("Bad id");
        }
        else
        {
            model = new();
        }

        await MapAsync(dto, model, cancellationToken);
        return model;
    }

    /// <inheritdoc />
    public Task MapAsync(Inspections.API.I.Inspection dto, Inspection model, CancellationToken cancellationToken)
    {
        model.Date = dto.Date;
        model.State = dto.State;
        model.Summary = dto.Summary;
        return Task.CompletedTask;
    }
}
