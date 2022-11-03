using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.BusinessLogic;

public class InspectionManager
    : Manager<Inspection>,
      IInspectionManager
{
    public InspectionManager(
        InspectionsDbContext inspectionsDbContext,
        IInspectionRepository repository)
        : base(inspectionsDbContext, repository)
    {
    }

    /// <inheritdoc />
    public async Task<Inspection?> GetByIdAsync(long inspectionId, long robotId, long customerId, CancellationToken cancellationToken)
    {
        Inspection? inspection = await GetByIdAsync(inspectionId, cancellationToken);
        return
            (inspection?.Robot?.Id == robotId)
            && (inspection.Robot?.Customer?.Id == customerId)
                ? inspection
                : null;
    }
}
