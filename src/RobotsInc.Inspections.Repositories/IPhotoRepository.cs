using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.Repositories;

public interface IPhotoRepository : IRepository<Photo>
{
    Task<long[]> FindIdsByNoteIdAsync(
        long noteId,
        CancellationToken cancellationToken);
}
