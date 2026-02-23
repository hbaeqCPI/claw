using Microsoft.AspNetCore.Authorization;
using R10.Core.Helpers;

namespace R10.Web.Security
{
    public class CPiServiceAccountHandler : AuthorizationHandler<CPiServiceAccountRequirement>
    {
        private readonly HttpContext? _httpContext;

        public CPiServiceAccountHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CPiServiceAccountRequirement requirement)
        {
            if (string.IsNullOrEmpty(requirement.ServiceAccountName) || !string.Equals(requirement.ServiceAccountName, context.User.GetEmail(), StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            if (!string.IsNullOrEmpty(requirement.ClientType) && !string.Equals(requirement.ClientType,
                (_httpContext?.Request.Headers.TryGetValue("X-Client-Type", out var clientTypes) ?? false) ? clientTypes.FirstOrDefault() : "", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
