using RobotsInc.Inspections.Models.Security;
using RobotsInc.Inspections.Repositories;
using RobotsInc.Inspections.Repositories.Security;

namespace RobotsInc.Inspections.BusinessLogic.Security;

public class UserManager
    : Manager<User>,
      IUserManager
{
    public UserManager(
        InspectionsDbContext inspectionsDbContext,
        IUserRepository repository)
        : base(inspectionsDbContext, repository)
    {
    }
}
