using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

using RobotsInc.Inspections.API.I.Security;

namespace RobotsInc.Inspections.Server.Security;

public class EmployeeConsultHandler : AuthorizationHandler<ConsultRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ConsultRequirement requirement)
    {
        if (context.User.IsInRole(ClaimTypes.Values.RoleEmployee))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
