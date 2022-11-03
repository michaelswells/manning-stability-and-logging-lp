using RobotsInc.Inspections.Models;

namespace RobotsInc.Inspections.Repositories;

public class CustomerRepository
    : Repository<Customer>,
      ICustomerRepository
{
    public CustomerRepository(InspectionsDbContext inspectionsDbContext)
        : base(inspectionsDbContext)
    {
    }
}
