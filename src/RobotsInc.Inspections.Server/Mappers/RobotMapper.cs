using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Server.Mappers;

public abstract class RobotMapper<TModel, TDto> : IRobotMapper<TModel, TDto>
    where TModel : Robot, new()
    where TDto : Inspections.API.I.Robot, new()
{
    private readonly IRobotRepository<TModel> _robotRepository;

    protected RobotMapper(
        IRobotRepository<TModel> robotRepository)
    {
        _robotRepository = robotRepository;
    }

    /// <inheritdoc />
    public virtual async Task<TDto> MapAsync(TModel model, CancellationToken cancellationToken)
    {
        TDto dto = new();
        await MapAsync(model, dto, cancellationToken);
        return dto;
    }

    /// <inheritdoc />
    public virtual Task MapAsync(TModel model, TDto dto, CancellationToken cancellationToken)
    {
        dto.Id = model.Id;
        dto.ManufacturingDate = model.ManufacturingDate;
        dto.SerialNumber = model.SerialNumber;
        dto.Description = model.Description;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task<TModel> MapAsync(TDto dto, CancellationToken cancellationToken)
    {
        TModel model;

        if (dto.Id != null)
        {
            TModel? robot = await _robotRepository.GetByIdAsync(dto.Id.Value, cancellationToken);
            model = robot ?? throw new BadHttpRequestException("Bad id");
        }
        else
        {
            model = new();
        }

        await MapAsync(dto, model, cancellationToken);
        return model;
    }

    /// <inheritdoc />
    public virtual Task MapAsync(TDto dto, TModel model, CancellationToken cancellationToken)
    {
        model.ManufacturingDate = dto.ManufacturingDate;
        model.SerialNumber = dto.SerialNumber;
        model.Description = dto.Description;
        return Task.CompletedTask;
    }
}
