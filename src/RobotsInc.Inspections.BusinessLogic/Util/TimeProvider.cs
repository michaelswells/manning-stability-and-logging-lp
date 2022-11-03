using System;

namespace RobotsInc.Inspections.BusinessLogic.Util;

public class TimeProvider : ITimeProvider
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;
}