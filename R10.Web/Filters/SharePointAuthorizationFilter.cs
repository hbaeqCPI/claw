using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Web.Extensions;
using R10.Web.Services;

namespace R10.Web.Filters
{
    public class SharePointAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authProvider = (IGraphAuthProvider?)context.HttpContext.RequestServices.GetService(typeof(IGraphAuthProvider));
            var settings = (IOptions<GraphSettings>?)context.HttpContext.RequestServices.GetService(typeof(IOptions<GraphSettings>));
            
            var request = context.HttpContext.Request;
            var user = context.HttpContext.User;

            if (authProvider != null && settings != null && user.GetDocumentStorageOption() == DocumentStorageOptions.SharePoint)
            {
                var graphSettings = settings.Value;

                if (!(await (authProvider.IsAuthenticated(graphSettings.SharePoint ?? new AuthProviderSettings(), (graphSettings.Site ?? new SiteSettings()).GetAuthenticationFlow(user), user.GetUserIdentifier()))))
                {
                    if (request.IsAjax())
                        context.Result = new UnauthorizedResult();
                    else
                        context.Result = new RedirectToActionResult("SharePoint", "Graph", new { area = "", returnUrl = request.GetEncodedPathAndQuery() });
                }
            }
        }
    }
}
