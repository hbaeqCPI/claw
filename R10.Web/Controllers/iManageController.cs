using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using R10.Web.Services.iManage;
using Microsoft.Extensions.Options;
using R10.Web.Extensions;
using R10.Web.Services;
using System.Globalization;
using DocuSign.eSign.Model;
using R10.Web.Models.IManageModels;
using R10.Web.Filters;
using System.Text;

namespace R10.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(ExceptionFilter))]
    public class iManageController : Controller
    {
        private readonly iManageSettings _iManageSettings;
        private readonly IiManageAuthProvider _authProvider;
        private IiManageClientFactory _iManageClientFactory;

        public iManageController(
            IOptions<iManageSettings> iManageSettings, 
            IiManageAuthProvider authProvider,
            IiManageClientFactory iManageClientFactory)
        {
            _iManageSettings = iManageSettings.Value;
            _authProvider = authProvider;
            _iManageClientFactory = iManageClientFactory;
        }

        private string BaseUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";

        public async Task<IActionResult> Auth(string? returnUrl)
        {
            var authFlow = _authProvider.GetAuthenticationFlow();
            var statusKey = "imanage-signin";

            if (authFlow == iManageAuthenticationFlow.Ropc)
            {
                await _authProvider.GetAuthDataByRopc();

                if (!string.IsNullOrEmpty(returnUrl))
                    return RedirectToLocal(returnUrl);
                else
                    return CloseWindow(statusKey);
            }
            else 
            { 
                var pkce = authFlow == iManageAuthenticationFlow.Pkce ? "1" : "";
                var authEndpoint = $"{_iManageSettings.ServerUrl}{_iManageSettings.AuthorizationEndpoint}";
                var clientId = _iManageSettings.AuthCodeFlow?.ClientId;
                var redirectUri = $"{BaseUrl}{_iManageSettings.AuthCodeFlow?.RedirectUri}";
                var state = Convert.ToBase64String((Encoding.UTF8.GetBytes($"{pkce},{returnUrl},{statusKey}")));

                if (pkce == "1")
                {
                    PkceParams pkceParams = PkceParams.GetParams();
                    HttpContext.Session.SetString("code_verifier", pkceParams.CodeVerifier);

                    return Redirect($"{authEndpoint}?response_type=code&client_id={clientId}&scope=user&redirect_uri={redirectUri}&state={state}&code_challenge={pkceParams.CodeChallenge}&code_challenge_method=S256");
                }
                else
                {
                    return Redirect($"{authEndpoint}?response_type=code&client_id={clientId}&scope=user&redirect_uri={redirectUri}&state={state}");
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            var logoutUri = await _authProvider.Logout(); //clear tokens

            //redirect to iManage logout endpoint to end existing session
            return Redirect(logoutUri);
        }

        //Auth code callback
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
