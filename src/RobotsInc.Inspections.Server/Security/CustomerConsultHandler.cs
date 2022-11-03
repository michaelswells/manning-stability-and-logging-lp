using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using RobotsInc.Inspections.API.I.Security;
using RobotsInc.Inspections.Server.API.I;

using Claim = System.Security.Claims.Claim;

namespace RobotsInc.Inspections.Server.Security;

public class CustomerConsultHandler : AuthorizationHandler<ConsultRequirement>
{
    public CustomerConsultHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        HttpContextAccessor = httpContextAccessor;
    }

    public IHttpContextAccessor HttpContextAccessor { get; }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ConsultRequirement requirement)
    {
        // if user is customer, then check link to requested customer
        if (context.User.IsInRole(ClaimTypes.Values.RoleCustomer))
        {
            RouteData routeData = HttpContextAccessor.HttpContext!.GetRouteData();
            if (routeData.Values.ContainsKey(CustomerController.IdentifierId))
            {
                object? customerId = routeData.Values[CustomerController.IdentifierId];
                Claim? claimForCustomerId = context.User.FindFirst(c => (c.Type == ClaimTypes.Customer) && (c.Value == customerId as string));
                if (claimForCustomerId != null)
                {
                    context.Succeed(requirement);
                }
            }
            else
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
