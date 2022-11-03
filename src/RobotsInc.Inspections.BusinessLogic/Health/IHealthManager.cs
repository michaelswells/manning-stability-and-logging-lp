using System.Threading;
using System.Threading.Tasks;

using RobotsInc.Inspections.API.I.Health;

namespace RobotsInc.Inspections.BusinessLogic.Health;

public interface IHealthManager
{
    /// <summary>
    ///     Checks the health of the service.
    /// </summary>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     A summary of the health status.
    /// </returns>
    Task<HealthResult> CheckHealthAsync(CancellationToken cancellationToken);
}
