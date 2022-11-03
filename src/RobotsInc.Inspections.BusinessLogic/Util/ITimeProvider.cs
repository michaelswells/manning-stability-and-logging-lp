using System;

namespace RobotsInc.Inspections.BusinessLogic.Util;

public interface ITimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
