namespace RobotsInc.Inspections.API.I.Health;

/// <summary>
///     Enum to describe the health status of the api.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    ///     Healthy: the service is available and can process incoming requests.
    /// </summary>
    HEALTHY = 1,

    /// <summary>
    ///     Unhealthy: the service is not in a healthy state and cannot accept incoming requests at this time.
    /// </summary>
    UNHEALTHY,

    /// <summary>
    ///     Service is not available: the service is closed for incoming requests outside of office hours.
    /// </summary>
    CLOSED
}
