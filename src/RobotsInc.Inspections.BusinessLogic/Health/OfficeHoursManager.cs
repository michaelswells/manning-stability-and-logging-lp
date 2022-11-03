using System;

namespace RobotsInc.Inspections.BusinessLogic.Health;

public class OfficeHoursManager : IOfficeHoursManager
{
    /// <inheritdoc />
    public bool IsWithinOfficeHours(DateTime dateTime)
    {
        DateTime localDateTime = dateTime.ToLocalTime();
        return
            (localDateTime.DayOfWeek != DayOfWeek.Saturday)
            && (localDateTime.DayOfWeek != DayOfWeek.Sunday)
            && (localDateTime.Hour is >= 9 and < 17);
    }
}
