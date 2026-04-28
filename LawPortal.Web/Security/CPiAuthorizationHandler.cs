using Microsoft.AspNetCore.Authorization;
using LawPortal.Core.Helpers;
using LawPortal.Core.Identity;

namespace LawPortal.Web.Security
{
    public class CPiAuthorizationHandler : AuthorizationHandler<CPiPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CPiPermissionRequirement requirement)
        {
            var statusOk = requirement.SystemStatus == null || requirement.SystemStatus == context.User.GetSystemStatus();

            if (!statusOk)
                return Task.CompletedTask;

            if (context.User.IsAdmin() && requirement.ForbidAdmin)
                return Task.CompletedTask;

            if (requirement.UserTypes.Count > 0 && !context.User.IsInUserTypes(requirement.UserTypes))
                return Task.CompletedTask;

            if (requirement.Roles == null)
            {
                if (context.User.IsInSystem(requirement.SystemType))
                    context.Succeed(requirement);
            }
            else
            {
                if (context.User.IsInRoles(requirement.SystemType, requirement.Roles))
                        context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
