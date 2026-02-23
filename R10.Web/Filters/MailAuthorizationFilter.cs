using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using R10.Core.Helpers;
using R10.Web.Extensions;
using R10.Web.Security;
using R10.Web.Services;

namespace R10.Web.Filters
{
    public class MailAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
    {
        protected readonly IGraphAuthProvider _authProvider;
        private readonly GraphSettings _graphSettings;
        private readonly IAuthorizationService _authService;

        public MailAuthorizationFilter(IGraphAuthProvider authProvider, IOptions<GraphSettings> graphSettings, IAuthorizationService authService)
        {
            _authProvider = authProvider;
            _graphSettings = graphSettings.Value;
            _authService = authService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var mailbox = (string?)request.Query["mailbox"] ?? (string?)request.Form["mailbox"];
            var mailSettings = _graphSettings.GetMailSettings(mailbox);

            if (!_graphSettings.HasMail || string.IsNullOrEmpty(mailbox) || mailSettings == null || !(await _authService.AuthorizeAsync(context.HttpContext.User, SharedAuthorizationPolicy.GetMailboxPolicyName(mailbox))).Succeeded)
                // ForbidResult() redirects to login page
                // APIs get a Status200OK result and a redirect to login page
                // context.Result = new ForbidResult();
                // Use StatusCodeResult to not trigger .NET authentication
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);

            else if (!(await (_authProvider.IsAuthenticated(_graphSettings.Mail, mailSettings.GraphClientAuthentication, context.HttpContext.User.GetUserIdentifier()))))
            {
                if (request.IsAjax())
                    context.Result = new UnauthorizedResult();
                else
                    context.Result = new RedirectToActionResult("Mail", "Graph", new { mailbox = mailSettings.MailboxName, returnUrl = request.GetEncodedPathAndQuery() });
            }
        }
    }
}
