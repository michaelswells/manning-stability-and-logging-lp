using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.API.I;

using Robot = RobotsInc.Inspections.Models.Robot;

namespace RobotsInc.Inspections.BusinessLogic;

public interface IRobotManager<TRobot> : IManager<TRobot>
    where TRobot : Robot
{
    Task<TRobot?> GetByIdAsync(long robotId, long customerId, CancellationToken cancellationToken);

    Task<PagedList<TRobot>> FindByCriteriaAsync(RobotSearchCriteria criteria, CancellationToken cancellationToken);
}
