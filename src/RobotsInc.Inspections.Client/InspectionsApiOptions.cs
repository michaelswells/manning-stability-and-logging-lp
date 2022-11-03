using System;

namespace RobotsInc.Inspections.Client;

/// <summary>
///     Configurable options for accessing the Inspections API.
/// </summary>
public class InspectionsApiOptions
{
    public const string Key = "InspectionsApi";

    /// <summary>
    ///     Set default values in the default constructor.
    /// </summary>
    public InspectionsApiOptions()
    {
        BaseAddress = "http://localhost:5000";
        Timeout = TimeSpan.FromSeconds(30);
    }

    public string BaseAddress { get; set; }

    public TimeSpan Timeout { get; set; }
}
