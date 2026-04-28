using Microsoft.AspNetCore.Authorization;
using LawPortal.Core.Helpers;
using LawPortal.Core.Identity;

namespace LawPortal.Web.Security
{
    public class ModuleAuthorizationHandler : AuthorizationHandler<ModulePermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ModulePermissionRequirement requirement)
        {
            var statusOk = requirement.SystemStatus == null || requirement.SystemStatus == context.User.GetSystemStatus();

            if (!statusOk)
                return Task.CompletedTask;

            if (context.User.IsAdmin() && requirement.ForbidAdmin)
                return Task.CompletedTask;

            if (requirement.UserTypes.Count > 0 && !context.User.IsInUserTypes(requirement.UserTypes))
                return Task.CompletedTask;

            if (requirement.SystemType == null && requirement.Roles == null)
            {
                if (context.User.IsModuleEnabled(requirement.Modules))
                    context.Succeed(requirement);
            }
            else if (requirement.SystemType == null)
            {
                if (context.User.IsInRoles(requirement.Roles) && context.User.IsModuleEnabled(requirement.Modules))
                    context.Succeed(requirement);
            }
            else if (requirement.Roles == null)
            {
                if (context.User.IsInSystem(requirement.SystemType) && context.User.IsModuleEnabled(requirement.Modules))
                    context.Succeed(requirement);
            }
            else
            {
                if (context.User.IsInRoles(requirement.SystemType, requirement.Roles) && context.User.IsModuleEnabled(requirement.Modules))
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
