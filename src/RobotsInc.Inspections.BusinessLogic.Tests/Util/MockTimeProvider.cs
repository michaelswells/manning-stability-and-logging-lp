using System;

namespace RobotsInc.Inspections.BusinessLogic.Util;

public class MockTimeProvider : ITimeProvider
{
    private readonly DateTime _dateTime;

    public MockTimeProvider(DateTime dateTime)
    {
        _dateTime = dateTime;
    }

    public DateTime Now => _dateTime.ToLocalTime();

    public DateTime UtcNow => _dateTime.ToUniversalTime();
}
