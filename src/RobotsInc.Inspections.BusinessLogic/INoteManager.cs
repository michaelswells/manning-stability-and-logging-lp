using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.BusinessLogic;

public interface INoteManager : IManager<Note>
{
    Task<Note?> GetByIdAsync(
        long noteId,
        long inspectionId,
        long robotId,
        long customerId,
        CancellationToken cancellationToken);
}
