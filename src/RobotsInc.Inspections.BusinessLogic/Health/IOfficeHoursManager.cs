using System;

namespace RobotsInc.Inspections.BusinessLogic.Health;

public interface IOfficeHoursManager
{
    /// <summary>
    ///     Indicates whether the given <paramref name="dateTime"/> is considered
    ///     to be within office hours.  Note that the incoming <paramref name="dateTime"/>
    ///     is converted into local time to compare to local office hours.
    /// </summary>
    /// <param name="dateTime">the given datetime</param>
    /// <returns>
    ///     Boolean indicating whether the datetime is within office hours.
    /// </returns>
    bool IsWithinOfficeHours(DateTime dateTime);
}