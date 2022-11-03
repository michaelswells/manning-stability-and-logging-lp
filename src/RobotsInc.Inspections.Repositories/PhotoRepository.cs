using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.Repositories;

public class PhotoRepository
    : Repository<Photo>,
      IPhotoRepository
{
    public PhotoRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }

    /// <inheritdoc />
    public async Task<long[]> FindIdsByNoteIdAsync(long noteId, CancellationToken cancellationToken)
    {
        long[] photoIds =
            await InspectionsDbContext
                .Photos
                .Where(p => p.Note!.Id == noteId)
                .Select(p => p.Id!.Value)
                .ToArrayAsync(cancellationToken);
        return photoIds;
    }
}
