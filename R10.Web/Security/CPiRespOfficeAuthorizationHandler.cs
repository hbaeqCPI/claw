using Microsoft.AspNetCore.Authorization;
using R10.Core.Helpers;

namespace R10.Web.Security
{
    public class CPiRespOfficeAuthorizationHandler : AuthorizationHandler<CPiRespOfficePermissionRequirement, string?>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CPiRespOfficePermissionRequirement requirement, string? respOffice)
        {
            var statusOk = requirement.SystemStatus == null || requirement.SystemStatus == context.User.GetSystemStatus();
            var roles = requirement.Roles;

            if (!statusOk)
                return Task.CompletedTask;

            if (context.User.IsAdmin() && requirement.ForbidAdmin)
                return Task.CompletedTask;

            if (roles == null)
            {
                if (context.User.IsInSystem(requirement.SystemType))
                    context.Succeed(requirement);
            }
            else
            {
                if (!string.IsNullOrEmpty(respOffice))
                    roles = roles.Select(role => role = $"{role}|{respOffice}").ToList();

                if (context.User.IsInRoles(requirement.SystemType, roles))
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
