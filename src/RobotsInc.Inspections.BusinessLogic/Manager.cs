using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.BusinessLogic;

public abstract class Manager<TModel> : IManager<TModel>
    where TModel : class
{
    protected Manager(
        InspectionsDbContext inspectionsDbContext,
        IRepository<TModel> repository)
    {
        InspectionsDbContext = inspectionsDbContext;
        Repository = repository;
    }

    public InspectionsDbContext InspectionsDbContext { get; }
    public IRepository<TModel> Repository { get; }

    /// <inheritdoc />
    public async Task<TModel?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        TModel? model;
        model = await Repository.GetByIdAsync(id, cancellationToken);

        /*IDbContextTransaction transaction = await InspectionsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            model = await Repository.GetByIdAsync(id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }*/

        return model;
    }

    /// <inheritdoc />
    public async Task SaveAsync(TModel model, CancellationToken cancellationToken)
    {
        await Repository.SaveOrUpdateAsync(model, cancellationToken);

        /*IDbContextTransaction transaction = await InspectionsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await Repository.SaveOrUpdateAsync(model, cancellationToken);
            await InspectionsDbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }*/
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TModel model, CancellationToken cancellationToken)
    {
        await Repository.DeleteAsync(model, cancellationToken);
        /*IDbContextTransaction transaction = await InspectionsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await Repository.DeleteAsync(model, cancellationToken);
            await InspectionsDbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }*/
    }
}
