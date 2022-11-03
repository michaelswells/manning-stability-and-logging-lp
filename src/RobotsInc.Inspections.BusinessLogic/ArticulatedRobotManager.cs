using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.BusinessLogic;

public class ArticulatedRobotManager
    : RobotManager<ArticulatedRobot>,
      IArticulatedRobotManager
{
    public ArticulatedRobotManager(
        InspectionsDbContext inspectionsDbContext,
        IArticulatedRobotRepository repository)
        : base(inspectionsDbContext, repository)
    {
    }
}
