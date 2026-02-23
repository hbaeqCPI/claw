using Microsoft.AspNetCore.Authorization;
using R10.Core.Helpers;
using R10.Core.Identity;

namespace R10.Web.Security
{
    public class SoftDocketAuthorizationHandler : AuthorizationHandler<SoftDocketPermissionRequirement, int?>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SoftDocketPermissionRequirement requirement, int? resource)
        {
            if (context.User.GetSystemStatus() != SystemStatusType.Active)
                return Task.CompletedTask;

            if (context.User.IsInRoles(requirement.SystemType, CPiPermissions.SoftDocket))
            {
                context.Succeed(requirement);
            }
            else if (context.User.IsSoftDocketUser())
            {
                // Attorney user types can only modify if repsonsibleId (resource) == entityId
                var entityId = context.User.GetEntityId();
                if (entityId == null || resource == entityId)
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
