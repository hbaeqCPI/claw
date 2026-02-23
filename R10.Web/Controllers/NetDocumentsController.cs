using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using R10.Web.Filters;
using R10.Web.Models;
using R10.Web.Models.NetDocumentsModels;
using R10.Web.Services.NetDocuments;
using System.Text;

namespace R10.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(ExceptionFilter))]
    public class NetDocumentsController : Controller
    {
        private readonly NetDocumentsSettings _netDocumentsSettings;
        private readonly INetDocumentsAuthProvider _authProvider;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public NetDocumentsController(
            IOptions<NetDocumentsSettings> netDocumentsSettings, 
            INetDocumentsAuthProvider authProvider,
            IStringLocalizer<SharedResource> localizer
            )
        {
            _netDocumentsSettings = netDocumentsSettings.Value;
            _authProvider = authProvider;
            _localizer = localizer;
        }

        private string BaseUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";

        public async Task<IActionResult> Auth(string? returnUrl)
        {
            var authFlow = _authProvider.GetAuthenticationFlow();
            var statusKey = "netdocuments-signin";

            if (authFlow == NetDocumentsAuthenticationFlow.ClientCredentials)
            {
                await _authProvider.GetAuthDataByClientCredentials();

                if (!string.IsNullOrEmpty(returnUrl))
                    return RedirectToLocal(returnUrl);
                else
                    return CloseWindow(statusKey);
            }
            else
            {
                var pkce = authFlow == NetDocumentsAuthenticationFlow.Pkce ? "1" : "";
                var redirectUri = $"{BaseUrl}{_netDocumentsSettings.AuthCodeFlow?.RedirectUri}";
                var state = Convert.ToBase64String((Encoding.UTF8.GetBytes($"{pkce},{returnUrl},{statusKey}")));
                var authUrl = $"{_netDocumentsSettings.AuthorizationURL}?response_type=code&client_id={_netDocumentsSettings.AuthCodeFlow?.ClientId}&scope={_netDocumentsSettings.AuthCodeFlow?.Scopes}&redirect_uri={redirectUri}&state={state}";

                if (pkce == "1")
                {
                    PkceParams pkceParams = PkceParams.GetParams();
                    HttpContext.Session.SetString("code_verifier", pkceParams.CodeVerifier);
                    authUrl = $"{authUrl}&code_challenge={pkceParams.CodeChallenge}&code_challenge_method=S256";
                }

                return Redirect(authUrl);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _authProvider.Logout(); //clear tokens
            return Ok(_localizer["You are now logged out of NetDocuments."].Value);
        }

        // Auth code callback
        public async Task<IActionResult> Authorize(string code, string state)
        {
            var states = Encoding.UTF8.GetString(Convert.FromBase64String(state)).Split(","); //pkce,returnUrl,statusKey
            var isPkce = states[0] == "1";
            var returnUrl = states[1];
            var statusKey = states[2];

            if (isPkce)
                await _authProvider.GetAuthDataByPkce(code);
            else
                await _authProvider.GetAuthDataByAuthorizationCode(code);

            if (!string.IsNullOrEmpty(returnUrl))
                return RedirectToLocal(returnUrl);

            if (!string.IsNullOrEmpty(statusKey))
                return CloseWindow(statusKey);

            return AuthenticationComplete();
        }

        private IActionResult CloseWindow(string statusKey)
        {
            // Set status in localStorage then close browser tab
            return Content($"<html><body><script>localStorage.setItem(\"{statusKey}\", \"ok\"); window.close();</script></body></html>", "text/html");
        }

        private IActionResult AuthenticationComplete()
        {
            // Message copied from interactive flow
            return Ok(_localizer["CPI successfully connected to your NetDocuments account. You may now close this window."].Value);
        }

        // Prevent external redirects
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
