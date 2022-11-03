using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.BusinessLogic;

public class AutomatedGuidedVehicleManager
    : RobotManager<AutomatedGuidedVehicle>,
      IAutomatedGuidedVehicleManager
{
    public AutomatedGuidedVehicleManager(
        InspectionsDbContext inspectionsDbContext,
        IAutomatedGuidedVehicleRepository repository)
        : base(inspectionsDbContext, repository)
    {
    }
}
