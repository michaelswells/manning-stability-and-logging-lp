using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.API.I.Health;
using RobotsInc.Inspections.BusinessLogic.Util;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.BusinessLogic.Health;

public class HealthManager : IHealthManager
{
    private readonly ILogger<HealthManager> _logger;
    private readonly ITimeProvider _timeProvider;
    private readonly IOfficeHoursManager _officeHoursManager;
    private readonly InspectionsDbContext _inspectionsDbContext;

    public HealthManager(
        ILogger<HealthManager> logger,
        ITimeProvider timeProvider,
        IOfficeHoursManager officeHoursManager,
        InspectionsDbContext inspectionsDbContext)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _officeHoursManager = officeHoursManager;
        _inspectionsDbContext = inspectionsDbContext;
    }

    /// <inheritdoc />
    public async Task<HealthResult> CheckHealthAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting health check.");

        HealthResult? health;

        // 1. check business hours
        DateTime now = _timeProvider.Now;
        bool open = _officeHoursManager.IsWithinOfficeHours(now);
        if (!open)
        {
            health =
                new()
                {
                    Status = HealthStatus.CLOSED,
                    Message = "Outside of office hours: business closed"
                };
        }
        else
        {
            // 2. if business hours ok, then check database connection
            bool databaseAccessible = await _inspectionsDbContext.Database.CanConnectAsync(cancellationToken);

            health =
                databaseAccessible
                    ? new()
                      {
                          Status = HealthStatus.HEALTHY,
                          Message = "Service up & running"
                      }
                    : new()
                      {
                          Status = HealthStatus.UNHEALTHY,
                          Message = "Database access is not available"
                      };
        }

        _logger.LogDebug("Health check finished.");
        return health;
    }
}
