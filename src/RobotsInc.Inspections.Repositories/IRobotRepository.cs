using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.API.I;

using Robot = RobotsInc.Inspections.Models.Robot;

namespace RobotsInc.Inspections.Repositories;

public interface IRobotRepository<TRobot> : IRepository<TRobot>
    where TRobot : Robot
{
    Task<PagedList<TRobot>> FindByCriteriaAsync(RobotSearchCriteria criteria, CancellationToken cancellationToken);
}
