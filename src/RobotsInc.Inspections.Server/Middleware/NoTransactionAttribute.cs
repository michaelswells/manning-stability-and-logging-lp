using System;

namespace RobotsInc.Inspections.Server.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class NoTransactionAttribute : Attribute
{
    public NoTransactionAttribute()
    {
    }
}
