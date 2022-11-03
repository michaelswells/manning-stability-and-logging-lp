using System.Threading;
using System.Threading.Tasks;

namespace RobotsInc.Inspections.BusinessLogic;

public interface IManager<TModel>
    where TModel : class
{
    Task<TModel?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task SaveAsync(TModel model, CancellationToken cancellationToken);
    Task DeleteAsync(TModel model, CancellationToken cancellationToken);
}
