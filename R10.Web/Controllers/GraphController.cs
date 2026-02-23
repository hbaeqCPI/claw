using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Web.Services;

namespace R10.Web.Controllers
{
    [Authorize]
    public class GraphController : Controller
    {
        private readonly SignInManager<CPiUser> _signInManager;
        protected readonly IGraphAuthProvider _authProvider;
        private readonly GraphSettings _graphSettings;
        private readonly ILogger<GraphController> _logger;

        public GraphController(
            SignInManager<CPiUser> signInManager,
            IGraphAuthProvider authProvider,
            IOptions<GraphSettings> graphSettings,
            ILogger<GraphController> logger)
        {
            _signInManager = signInManager;
            _authProvider = authProvider;
            _graphSettings = graphSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Authorization Code flow and
        /// On Behalf Of flow authentication and user consent
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="flow"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> Auth(string provider, GraphClientAuthenticationFlow flow, string? returnUrl = null, string? statusKey = null)
        {
            //default localStorage key for checking authentication status
            statusKey = statusKey ?? "graph-signin";

            //on behalf of flow
            if (flow == GraphClientAuthenticationFlow.OnBehalfOf)
            {
                provider = string.IsNullOrEmpty(provider) ? _graphSettings.Mail.ProviderName : provider;
                var redirectUrl = Url.Action(nameof(AuthCallback), "Graph", new { returnUrl, statusKey });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return Challenge(properties, provider);
            }

            //authorization code flow
            var request = HttpContext.Request;
            var authProvider = provider.ToLower() == _graphSettings.Mail.ProviderName.ToLower() ? _graphSettings.Mail : _graphSettings.SharePoint;
            var redirectUri = $"{request.Scheme}://{request.Host}{request.PathBase}{authProvider.RedirectUri}";
            var prompt = authProvider.AlwaysPromptUserConsent ? "&prompt=consent" : "";

            return Redirect($"{authProvider.AuthorizationEndpoint}?client_id={authProvider.ClientId}&response_type=code&redirect_uri={redirectUri}&response_mode=query&scope={authProvider.Scopes}&state={authProvider.ProviderName},{returnUrl},{statusKey}&sso_reload=true{prompt}");
        }

        //sharepoint sign-in
        public async Task<IActionResult> SharePoint(string? returnUrl = null, string? statusKey = null)
        {
            return await Auth(_graphSettings.SharePoint.ProviderName, _graphSettings.Site.GetAuthenticationFlow(User), returnUrl, statusKey);
        }

        //mail sign-in
        public async Task<IActionResult> Mail(string mailbox, string? returnUrl = null, string? statusKey = null)
        {
            var mailSettings = _graphSettings.GetMailSettings(mailbox);
            return await Auth(_graphSettings.Mail.ProviderName, mailSettings.GraphClientAuthentication, returnUrl, statusKey);
        }

        //on behalf of flow callback
        public async Task<IActionResult> AuthCallback(string? returnUrl = null, string? remoteError = null, string? statusKey = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                _logger.LogError(remoteError);
                return BadRequest(remoteError);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return BadRequest();

            try
            {
                //must enable ID tokens (used for implicit and hybrid flows) in app registration
                var idToken = info.AuthenticationTokens.First(f => f.Name == "id_token").Value;
                var expiresOn = DateTimeOffset.Parse(info.AuthenticationTokens.First(f => f.Name == "expires_at").Value);

                _authProvider.SetIdToken(info.LoginProvider, GraphClientAuthenticationFlow.OnBehalfOf, User.GetUserIdentifier(), idToken, expiresOn);
                
                //call if need to save tokens to db
                //await _signInManager.UpdateExternalAuthenticationTokensAsync(info); 
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return RedirectToLocal(returnUrl);

            if (!string.IsNullOrEmpty(statusKey))
                return CloseWindow(statusKey);

            return AuthenticationComplete();
        }

        //authorization code flow callback
        public async Task<IActionResult>  Authorize(string code, string state)
        {
            var states = state.Split(","); //providerName,returnUrl,statusKey
            var authProvider = states[0].ToLower() == _graphSettings.Mail.ProviderName.ToLower() ? _graphSettings.Mail : _graphSettings.SharePoint;
            
            //acquire and save token
            await _authProvider.GetTokenByAuthorizationCode(authProvider, User.GetUserIdentifier(), code);

            if (!string.IsNullOrEmpty(states[1]))
                return RedirectToLocal(states[1]);

            if (!string.IsNullOrEmpty(states[2]))
                return CloseWindow(states[2]);

            return AuthenticationComplete();
        }

        private IActionResult CloseWindow(string statusKey)
        {
            //set status in localStorage then close browser tab
            return Content($"<html><body><script>localStorage.setItem(\"{statusKey}\", \"ok\"); window.close();</script></body></html>", "text/html");
        }

        private IActionResult AuthenticationComplete()
        {
            //message copied from interactive flow
            return Ok("Authentication complete. You can return to the application. Feel free to close this browser tab.");
        }

        //prevent external redirects
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
