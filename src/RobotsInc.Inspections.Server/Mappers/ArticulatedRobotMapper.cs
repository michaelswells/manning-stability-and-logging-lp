using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Server.Mappers;

public class ArticulatedRobotMapper : RobotMapper<ArticulatedRobot, Inspections.API.I.ArticulatedRobot>
{
    public ArticulatedRobotMapper(IArticulatedRobotRepository robotRepository)
        : base(robotRepository)
    {
    }

    /// <inheritdoc />
    public override async Task MapAsync(ArticulatedRobot model, Inspections.API.I.ArticulatedRobot dto, CancellationToken cancellationToken)
    {
        await base.MapAsync(model, dto, cancellationToken);
        dto.NrOfJoints = model.NrOfJoints;
    }

    /// <inheritdoc />
    public override async Task MapAsync(Inspections.API.I.ArticulatedRobot dto, ArticulatedRobot model, CancellationToken cancellationToken)
    {
        await base.MapAsync(dto, model, cancellationToken);
        model.NrOfJoints = dto.NrOfJoints;
    }
}