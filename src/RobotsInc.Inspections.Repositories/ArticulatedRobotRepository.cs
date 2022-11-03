using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.Repositories;

public class ArticulatedRobotRepository
    : RobotRepository<ArticulatedRobot>, IArticulatedRobotRepository
{
    public ArticulatedRobotRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }
}
