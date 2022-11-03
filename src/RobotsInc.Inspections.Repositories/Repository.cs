using System.Threading;
using System.Threading.Tasks;

namespace RobotsInc.Inspections.Repositories;

public abstract class Repository<TModel> : IRepository<TModel>
    where TModel : class
{
    protected Repository(InspectionsDbContext inspectionsDbContext)
    {
        InspectionsDbContext = inspectionsDbContext;
    }

    public InspectionsDbContext InspectionsDbContext { get; }

    /// <inheritdoc />
    public async Task<TModel?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        TModel? model =
            await InspectionsDbContext
                .GetDbSet<TModel>()
                .FindAsync(new object[] { id }, cancellationToken);
        return model;
    }

    /// <inheritdoc />
    public Task SaveOrUpdateAsync(TModel model, CancellationToken cancellationToken)
    {
        InspectionsDbContext
            .GetDbSet<TModel>()
            .Update(model);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(TModel model, CancellationToken cancellationToken)
    {
        InspectionsDbContext
            .GetDbSet<TModel>()
            .Remove(model);
        return Task.CompletedTask;
    }
}
