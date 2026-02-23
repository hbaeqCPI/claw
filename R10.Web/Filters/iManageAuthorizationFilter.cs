using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Web.Extensions;
using R10.Web.Services;
using R10.Web.Services.iManage;

namespace R10.Web.Filters
{
    public class iManageAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var user = context.HttpContext.User;

            // Ignore if user is not authenticated to prevent
            // instantiating IiManageAuthProvider (scoped) DI without user context
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var authProvider = (IiManageAuthProvider?)context.HttpContext.RequestServices.GetService(typeof(IiManageAuthProvider));
                if (authProvider != null && user.GetDocumentStorageOption() == DocumentStorageOptions.iManage)
                {
                    var authData = await authProvider.AcquireTokenSilent(authProvider.GetAuthenticationFlow());
                    if (authData == null)
                    {
                        if (request.IsAjax())
                            context.Result = new UnauthorizedResult();
                        else
                            //redirect user to login page when using auth code flow (interactive)
                            context.Result = new RedirectToActionResult("Auth", "iManage", new { area = "", returnUrl = request.GetEncodedPathAndQuery() });
                    }
                }
            }
        }
    }
}
