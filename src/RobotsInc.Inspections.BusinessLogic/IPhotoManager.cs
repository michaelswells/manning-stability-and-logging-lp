using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.BusinessLogic;

public interface IPhotoManager : IManager<Photo>
{
    Task<Photo?> GetByIdAsync(
        long photoId,
        long noteId,
        long inspectionId,
        long robotId,
        long customerId,
        CancellationToken cancellationToken);

    Task<long[]> FindIdsByNoteIdAsync(
        long noteId,
        CancellationToken cancellationToken);
}
