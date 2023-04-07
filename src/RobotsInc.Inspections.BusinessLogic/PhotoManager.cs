using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.BusinessLogic;

public class PhotoManager
    : Manager<Photo>,
      IPhotoManager
{
    public PhotoManager(
        InspectionsDbContext inspectionsDbContext,
        IPhotoRepository photoRepository)
        : base(inspectionsDbContext, photoRepository)
    {
        PhotoRepository = photoRepository;
    }

    public IPhotoRepository PhotoRepository { get; }

    /// <inheritdoc />
    public async Task<Photo?> GetByIdAsync(long photoId, long noteId, long inspectionId, long robotId, long customerId, CancellationToken cancellationToken)
    {
        Photo? photo = await GetByIdAsync(photoId, cancellationToken);
        return
            (photo?.Note?.Id == noteId)
            && (photo.Note.Inspection?.Id == inspectionId)
            && (photo.Note.Inspection.Robot?.Id == robotId)
            && (photo.Note.Inspection.Robot.Customer?.Id == customerId)
                ? photo
                : null;
    }

    /// <inheritdoc />
    public async Task<long[]> FindIdsByNoteIdAsync(long noteId, CancellationToken cancellationToken)
    {
        long[] photoIds;
        photoIds = await PhotoRepository.FindIdsByNoteIdAsync(noteId, cancellationToken);

        /*IDbContextTransaction transaction = await InspectionsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            photoIds = await PhotoRepository.FindIdsByNoteIdAsync(noteId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }*/

        return photoIds;
    }
}
