using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.BusinessLogic;

public class NoteManager
    : Manager<Note>,
      INoteManager
{
    public NoteManager(
        InspectionsDbContext inspectionsDbContext,
        INoteRepository repository)
        : base(inspectionsDbContext, repository)
    {
    }

    /// <inheritdoc />
    public async Task<Note?> GetByIdAsync(long noteId, long inspectionId, long robotId, long customerId, CancellationToken cancellationToken)
    {
        Note? note = await GetByIdAsync(noteId, cancellationToken);
        return
            (note?.Inspection?.Id == inspectionId)
            && (note.Inspection?.Robot?.Id == robotId)
            && (note.Inspection.Robot?.Customer?.Id == customerId)
                ? note
                : null;
    }
}
