using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using RobotsInc.Inspections.API.I;

using ArticulatedRobot = RobotsInc.Inspections.Models.ArticulatedRobot;
using AutomatedGuidedVehicle = RobotsInc.Inspections.Models.AutomatedGuidedVehicle;
using Robot = RobotsInc.Inspections.Models.Robot;

namespace RobotsInc.Inspections.Repositories;

public class RobotRepository<TRobot>
    : Repository<TRobot>,
      IRobotRepository<TRobot>
    where TRobot : Robot
{
    public RobotRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }

    /// <inheritdoc />
    public async Task<PagedList<TRobot>> FindByCriteriaAsync(RobotSearchCriteria criteria, CancellationToken cancellationToken)
    {
        IQueryable<TRobot> queryable = CreateQueryable(criteria);

        Debug.Assert(criteria.Page != null, "Page should not be null, already validated.");
        Debug.Assert(criteria.PageSize != null, "PageSize should not be null, already validated");
        int recordsToSkip = criteria.Page.Value * criteria.PageSize.Value;
        int recordsToTake = criteria.PageSize.Value;

        TRobot[] robots = await queryable.Skip(recordsToSkip).Take(recordsToTake).ToArrayAsync(cancellationToken);
        int count = await queryable.CountAsync(cancellationToken);

        PagedList<TRobot> result =
            new()
            {
                Page = criteria.Page,
                PageSize = criteria.PageSize,
                Items = robots,
                TotalCount = count,
                TotalPages = (count % criteria.PageSize) + 1
            };

        return result;
    }

    private IQueryable<TRobot> CreateQueryable(RobotSearchCriteria criteria)
    {
        IQueryable<TRobot> queryable = InspectionsDbContext.GetDbSet<TRobot>();

        if (criteria.CustomerId != null)
        {
            queryable = queryable.Where(r => r.Customer!.Id == criteria.CustomerId.Value);
        }

        if (criteria.CustomerIds != null)
        {
            queryable = queryable.Where(r => criteria.CustomerIds.Contains(r.Customer!.Id!.Value));
        }

        if (criteria.RobotType != null)
        {
            switch (criteria.RobotType.Value)
            {
                case RobotType.ARTICULATED_ROBOT:
                    queryable = queryable.Where(r => r is ArticulatedRobot);
                    break;
                case RobotType.AUTOMATED_GUIDED_VEHICLE:
                    queryable = queryable.Where(r => r is AutomatedGuidedVehicle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (criteria.ManufacturingDateFrom != null)
        {
            queryable = queryable.Where(r => criteria.ManufacturingDateFrom <= r.ManufacturingDate);
        }

        if (criteria.ManufacturingDateTo != null)
        {
            queryable = queryable.Where(r => r.ManufacturingDate <= criteria.ManufacturingDateTo);
        }

        queryable = queryable.OrderByDescending(r => r.ManufacturingDate);

        return queryable;
    }
}
