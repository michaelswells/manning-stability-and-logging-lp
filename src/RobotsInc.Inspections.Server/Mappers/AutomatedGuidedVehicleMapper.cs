using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Server.Mappers;

public class AutomatedGuidedVehicleMapper : RobotMapper<AutomatedGuidedVehicle, Inspections.API.I.AutomatedGuidedVehicle>
{
    public AutomatedGuidedVehicleMapper(IAutomatedGuidedVehicleRepository robotRepository)
        : base(robotRepository)
    {
    }

    /// <inheritdoc />
    public override async Task MapAsync(AutomatedGuidedVehicle model, Inspections.API.I.AutomatedGuidedVehicle dto, CancellationToken cancellationToken)
    {
        await base.MapAsync(model, dto, cancellationToken);
        dto.ChargingType = model.ChargingType;
        dto.NavigationType = model.NavigationType;
    }

    /// <inheritdoc />
    public override async Task MapAsync(Inspections.API.I.AutomatedGuidedVehicle dto, AutomatedGuidedVehicle model, CancellationToken cancellationToken)
    {
        await base.MapAsync(dto, model, cancellationToken);
        model.ChargingType = dto.ChargingType;
        model.NavigationType = dto.NavigationType;
    }
}
