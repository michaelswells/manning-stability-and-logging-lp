using System.Threading;
using System.Threading.Tasks;

namespace RobotsInc.Inspections.Server.Mappers;

public interface IMapper<TModel, TDto>
    where TModel : new()
    where TDto : new()
{
    /// <summary>
    ///     Map <paramref name="model" /> of type <typeparamref name="TModel" /> to a new dto of type
    ///     <typeparamref name="TDto" /> that is returned as a result.
    /// </summary>
    /// <param name="model">the given model</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    ///     A newly created dto of type <typeparamref name="TDto" /> with its properties configured based on the given
    ///     <paramref name="model" />.
    /// </returns>
    Task<TDto> MapAsync(TModel model, CancellationToken cancellationToken);

    /// <summary>
    ///     Map <paramref name="model" /> of type <typeparamref name="TModel" /> to <paramref name="dto" />
    ///     of type <typeparamref name="TDto" />.
    /// </summary>
    /// <param name="model">the given model</param>
    /// <param name="dto">the given dto</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    ///     Returns <see cref="Task" /> because the method is async.
    /// </returns>
    Task MapAsync(TModel model, TDto dto, CancellationToken cancellationToken);

    /// <summary>
    ///     Map <paramref name="dto" /> of type <typeparamref name="TDto" /> to a new model of type
    ///     <typeparamref name="TModel" /> that is returned as a result.
    /// </summary>
    /// <param name="dto">the given dto</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    ///     A newly created model of type <typeparamref name="TModel" /> with its properties configured based on the
    ///     given <paramref name="dto" />.
    /// </returns>
    Task<TModel> MapAsync(TDto dto, CancellationToken cancellationToken);

    /// <summary>
    ///     Map <paramref name="dto" /> of type <typeparamref name="TDto" /> to <paramref name="model" /> of type
    ///     <typeparamref name="TModel" />.
    /// </summary>
    /// <param name="dto">the given dto</param>
    /// <param name="model">the given model</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    ///     Returns <see cref="Task" /> because the method is async.
    /// </returns>
    Task MapAsync(TDto dto, TModel model, CancellationToken cancellationToken);
}
