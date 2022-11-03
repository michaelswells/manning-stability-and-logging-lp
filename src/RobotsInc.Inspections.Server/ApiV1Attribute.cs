using System;

using Microsoft.AspNetCore.Mvc;

namespace RobotsInc.Inspections.Server;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ApiV1Attribute : ApiVersionAttribute
{
    public ApiV1Attribute()
        : base(new ApiVersion(1, 0))
    {
    }
}