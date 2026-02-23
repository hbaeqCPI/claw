using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Web.Extensions;
using R10.Web.Services.NetDocuments;

namespace R10.Web.Filters
{
    public class NetDocumentsAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var user = context.HttpContext.User;

            // Ignore if user is not authenticated to prevent
            // instantiating INetDocumentsAuthProvider (scoped) DI without user context
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var authProvider = (INetDocumentsAuthProvider?)context.HttpContext.RequestServices.GetService(typeof(INetDocumentsAuthProvider));                
                if (authProvider != null && user.GetDocumentStorageOption() == DocumentStorageOptions.NetDocuments)
                {
                    // Check if token has expired
                    var authData = await authProvider.AcquireTokenSilent(authProvider.GetAuthenticationFlow());
                    if (authData == null)
                    {
                        if (request.IsAjax())
                            context.Result = new UnauthorizedResult();
                        else
                            // Redirect user to login page when using auth code flow (interactive)
                            context.Result = new RedirectToActionResult("Auth", "NetDocuments", new { area = "", returnUrl = request.GetEncodedPathAndQuery() });
                    }
                }
            }
        }
    }
}
