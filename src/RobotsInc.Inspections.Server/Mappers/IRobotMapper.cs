using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.Server.Mappers;

public interface IRobotMapper<TModel, TDto> : IMapper<TModel, TDto>
    where TModel : Robot, new()
    where TDto : Inspections.API.I.Robot, new()
{
}
