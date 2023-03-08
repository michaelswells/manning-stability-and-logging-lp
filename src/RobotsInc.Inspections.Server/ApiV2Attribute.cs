using System;

using Asp.Versioning;

namespace RobotsInc.Inspections.Server;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ApiV2Attribute : ApiVersionAttribute
{
    public ApiV2Attribute()
        : base(new ApiVersion(2, 0))
    {
    }
}
