using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.API.I;
using RobotsInc.Inspections.Repositories;

using Robot = RobotsInc.Inspections.Models.Robot;

namespace RobotsInc.Inspections.BusinessLogic;

public class RobotManager<TRobot>
    : Manager<TRobot>,
      IRobotManager<TRobot>
    where TRobot : Robot
{
    public RobotManager(
        InspectionsDbContext inspectionsDbContext,
        IRobotRepository<TRobot> robotRepository)
        : base(inspectionsDbContext, robotRepository)
    {
        RobotRepository = robotRepository;
    }

    public IRobotRepository<TRobot> RobotRepository { get; }

    /// <inheritdoc />
    public async Task<TRobot?> GetByIdAsync(long robotId, long customerId, CancellationToken cancellationToken)
    {
        TRobot? robot = await GetByIdAsync(robotId, cancellationToken);
        return robot?.Customer?.Id == customerId
                   ? robot
                   : null;
    }

    /// <inheritdoc />
    public async Task<PagedList<TRobot>> FindByCriteriaAsync(RobotSearchCriteria criteria, CancellationToken cancellationToken)
    {
        PagedList<TRobot> robots = await RobotRepository.FindByCriteriaAsync(criteria, cancellationToken);
        return robots;

        /*IDbContextTransaction transaction = await InspectionsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            PagedList<TRobot> robots = await RobotRepository.FindByCriteriaAsync(criteria, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return robots;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }*/
    }
}
