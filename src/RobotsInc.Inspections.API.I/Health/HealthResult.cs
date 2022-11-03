namespace RobotsInc.Inspections.API.I.Health;

public class HealthResult
{
    public HealthStatus Status { get; set; }

    public string? Message { get; set; }
}