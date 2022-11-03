using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.Repositories;

public class InspectionRepository
    : Repository<Inspection>,
      IInspectionRepository
{
    public InspectionRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }
}
