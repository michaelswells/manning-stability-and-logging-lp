using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.BusinessLogic;

public class CustomerManager
    : Manager<Customer>,
      ICustomerManager
{
    public CustomerManager(
        InspectionsDbContext inspectionsDbContext,
        ICustomerRepository repository)
        : base(inspectionsDbContext, repository)
    {
    }
}
