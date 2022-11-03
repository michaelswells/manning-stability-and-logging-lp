using System.Threading;
using System.Threading.Tasks;

namespace RobotsInc.Inspections.Repositories;

public interface IRepository<TModel>
    where TModel : class
{
    Task<TModel?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task SaveOrUpdateAsync(TModel model, CancellationToken cancellationToken);
    Task DeleteAsync(TModel model, CancellationToken cancellationToken);
}