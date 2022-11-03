using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.Repositories;

public class AutomatedGuidedVehicleRepository
    : RobotRepository<AutomatedGuidedVehicle>,
      IAutomatedGuidedVehicleRepository
{
    public AutomatedGuidedVehicleRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }
}
