using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.BusinessLogic;

public interface IInspectionManager : IManager<Inspection>
{
    Task<Inspection?> GetByIdAsync(long inspectionId, long robotId, long customerId, CancellationToken cancellationToken);
}
