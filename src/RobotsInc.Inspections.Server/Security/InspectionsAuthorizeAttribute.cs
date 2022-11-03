using Microsoft.AspNetCore.Authorization;

namespace RobotsInc.Inspections.Server.Security;

public class InspectionsAuthorizeAttribute : AuthorizeAttribute
{
    public InspectionsAuthorizeAttribute()
    {
    }

    public InspectionsAuthorizeAttribute(Policy policy)
    {
        Policy = policy.ToString();
    }
}
