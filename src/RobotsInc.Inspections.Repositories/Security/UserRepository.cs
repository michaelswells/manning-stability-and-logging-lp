using RobotsInc.Inspections.Models.Security;

namespace RobotsInc.Inspections.Repositories.Security;

public class UserRepository
    : Repository<User>,
      IUserRepository
{
    public UserRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }
}
