using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.Repositories;

public class NoteRepository
    : Repository<Note>,
      INoteRepository
{
    public NoteRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }
}
