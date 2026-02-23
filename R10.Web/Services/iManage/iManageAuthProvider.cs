using Azure;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Web.Extensions;
using R10.Web.Models.IManageModels;
using System.Drawing.Drawing2D;
using System.Net;
using System.Security;
using System.Security.Claims;
using System.Text.Json;

namespace R10.Web.Services.iManage
{
    public class iManageAuthProvider : IiManageAuthProvider
    {
        private readonly IHttpContextAccessor _context;
        private readonly iManageSettings _iManageSettings;
        private readonly UserManager<CPiUser> _userManager;
        private readonly ClaimsPrincipal _user;

        public iManageAuthProvider(
            IHttpContextAccessor context, 
            IOptions<iManageSettings> iManageSettings,
            UserManager<CPiUser> userManager,
            ClaimsPrincipal user
            )
        {
            _context = context;
            _iManageSettings = iManageSettings.Value;
            _userManager = userManager;
            _user = user;
        }

        //auth token cookie
        private string GetCookieName(iManageAuthenticationFlow authFlow) => $"CPI_iManage_{_user.GetUserIdentifier()}_{(int)authFlow}";

        //for saving refresh token to tblCPiUserTokens
        private string GetTokenProvider(iManageAuthenticationFlow authFlow) => $"iManage_{(int)authFlow}";
        private string GetTokenName() => $"refresh_token";

        private string BaseUrl => $"{_context.HttpContext?.Request.Scheme}://{_context.HttpContext?.Request.Host}{_context.HttpContext?.Request.PathBase}";
        private string TokenUri => $"{_iManageSettings.ServerUrl}{_iManageSettings.TokenEndpoint}";

        public async Task<AuthData?> GetAuthDataByAuthorizationCode(string code)
        {
            var authFlow = iManageAuthenticationFlow.AuthorizationCode;
            var redirectUri = $"{BaseUrl}{_iManageSettings.AuthCodeFlow?.RedirectUri ?? ""}";

            var payload = new Dictionary<string, string>();
            payload.Add("client_id", _iManageSettings.AuthCodeFlow?.ClientId ?? "");
            payload.Add("client_secret", _iManageSettings.AuthCodeFlow?.ClientSecret ?? "");
            payload.Add("redirect_uri", redirectUri);
            payload.Add("grant_type", "authorization_code");
            payload.Add("code", code);
            payload.Add("scope", _iManageSettings.AuthCodeFlow?.Scopes ?? "");

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, TokenUri) { Content = new FormUrlEncodedContent(payload) };
            var response = await client.SendAsync(request);
            var requestStart = DateTime.Now;
            var result = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var authResult = JsonSerializer.Deserialize<AuthResult>(result);
                var token = authResult?.XAuthToken ?? "";
                var apiData = await GetApiData(client, token);
                var authData = new AuthData(token, apiData);

                SaveAuthData(authData, requestStart.AddSeconds(authResult?.ExpiresIn ?? 0), authFlow);

                if (!string.IsNullOrEmpty(authResult?.RefreshToken))
                    await SaveRefreshToken(authResult.RefreshToken, authFlow);

                return authData;
            }

            throw new iManageServiceException(await response.GetErrorMessage(), response.StatusCode);
        }

        public async Task<AuthData?> GetAuthDataByPkce(string code)
        {
            var authFlow = iManageAuthenticationFlow.Pkce;
            var redirectUri = $"{BaseUrl}{_iManageSettings.AuthCodeFlow?.RedirectUri ?? ""}";

            var payload = new Dictionary<string, string>();
            payload.Add("client_id", _iManageSettings.AuthCodeFlow?.ClientId ?? "");
            payload.Add("client_secret", _iManageSettings.AuthCodeFlow?.ClientSecret ?? "");
            payload.Add("code_verifier", _context.HttpContext?.Session.GetString("code_verifier") ?? "");
            payload.Add("redirect_uri", redirectUri);
            payload.Add("grant_type", "authorization_code");
            payload.Add("code", code);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, TokenUri) { Content = new FormUrlEncodedContent(payload) };
            var requestStart = DateTime.Now;
            var response = await client.SendAsync(request);
            var result = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var authResult = JsonSerializer.Deserialize<AuthResult>(result);
                var token = authResult?.XAuthToken ?? "";
                var apiData = await GetApiData(client, token);
                var authData = new AuthData(token, apiData);

                SaveAuthData(authData, requestStart.AddSeconds(authResult?.ExpiresIn ?? 0), authFlow);

                if (!string.IsNullOrEmpty(authResult?.RefreshToken))
                    await SaveRefreshToken(authResult.RefreshToken, authFlow);

                return authData;
            }

            throw new iManageServiceException(await response.GetErrorMessage(), response.StatusCode);
        }

        public async Task<AuthData?> GetAuthDataByRopc()
        {
            var authFlow = iManageAuthenticationFlow.Ropc;

            var payload = new Dictionary<string, string>();
            payload.Add("client_id", _iManageSettings.RopcFlow?.ClientId ?? "");
            payload.Add("client_secret", _iManageSettings.RopcFlow?.ClientSecret ?? "");
            payload.Add("username", _iManageSettings.RopcFlow?.UserName ?? "");
            payload.Add("password", _iManageSettings.RopcFlow?.Password ?? "");
            payload.Add("grant_type", "password");
            payload.Add("scope", _iManageSettings.RopcFlow?.Scopes ?? "");

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, TokenUri) { Content = new FormUrlEncodedContent(payload) };
            var requestStart = DateTime.Now;
            var response = await client.SendAsync(request);
            var result = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var authResult = JsonSerializer.Deserialize<AuthResult>(result);
                var token = authResult?.XAuthToken ?? "";
                var apiData = await GetApiData(client, token);
                var authData = new AuthData(token, apiData);

                SaveAuthData(authData, requestStart.AddSeconds(authResult?.ExpiresIn ?? 0), authFlow);

                if (!string.IsNullOrEmpty(authResult?.RefreshToken))
                    await SaveRefreshToken(authResult.RefreshToken, authFlow);

                return authData;
            }

            throw new iManageServiceException(await response.GetErrorMessage(), response.StatusCode);
        }

        public async Task<string> Logout()
        {
            var logoutUri = $"{_iManageSettings.ServerUrl}{_iManageSettings.LogoutEndpoint}";
            var http = new HttpClient();
            var authFlows = Enum.GetValues(typeof(iManageAuthenticationFlow)).Cast<iManageAuthenticationFlow>();
            var user = await _userManager.GetUserAsync(_user);

            foreach (var authFlow in authFlows)
            {
                var authData = GetTokenFromCache(authFlow);

                if (authData != null)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, logoutUri);

                    request.Headers.Add("X-Auth-Token", authData.XAuthToken);
                    HttpResponseMessage response = await http.SendAsync(request);
                }

                //delete cookies
                _context.HttpContext?.Response.Cookies.Delete(GetCookieName(authFlow));

                //delete refresh tokens
                if (user != null)
                    await _userManager.RemoveAuthenticationTokenAsync(user, GetTokenProvider(authFlow), GetTokenName());
            }

            return logoutUri;
        }

        public async Task<bool> IsTokenExpired(iManageAuthenticationFlow authFlow)
        {
            var authData = GetTokenFromCache(authFlow);
            var refreshToken = "";

            if (authData == null)
                refreshToken = await GetRefreshToken(authFlow);

            return (authData == null && string.IsNullOrEmpty(refreshToken));
        }

        private AuthData? GetTokenFromCache(iManageAuthenticationFlow authFlow)
        {
            var cookie = _context.HttpContext?.Request.Cookies[GetCookieName(authFlow)];

            if (!string.IsNullOrEmpty(cookie))
                return JsonSerializer.Deserialize<AuthData>(cookie);

            return null;
        }

        private async Task<string?> GetRefreshToken(iManageAuthenticationFlow authFlow)
        {
            var refreshToken = "";
            var user = await _userManager.GetUserAsync(_user);

            if (user != null)
                refreshToken = await _userManager.GetAuthenticationTokenAsync(user, GetTokenProvider(authFlow), GetTokenName());

            return refreshToken;
        }

        /// <summary>
        /// Attempt to get a token from the cache 
        /// or refresh it silently if needed
        /// </summary>
        /// <returns></returns>
        public async Task<AuthData?> AcquireTokenSilent(iManageAuthenticationFlow authFlow)
        {
            var authData = GetTokenFromCache(authFlow);

            if (authData == null)
            {
                var refreshToken = await GetRefreshToken(authFlow);

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var payload = new Dictionary<string, string>();
                    payload.Add("grant_type", "refresh_token");
                    payload.Add("refresh_token", refreshToken);
                    payload.Add("scope", GetScopes(authFlow));

                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, TokenUri) { Content = new FormUrlEncodedContent(payload) };
                    var response = await client.SendAsync(request);
                    var requestStart = DateTime.Now;
                    var result = response.Content.ReadAsStringAsync().Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var authResult = JsonSerializer.Deserialize<AuthResult>(result);
                        var token = authResult?.XAuthToken ?? "";
                        var apiData = await GetApiData(client, token);

                        authData = new AuthData(token, apiData);
                        SaveAuthData(authData, requestStart.AddSeconds(authResult?.ExpiresIn ?? 0), authFlow);

                        if (!string.IsNullOrEmpty(authResult?.RefreshToken))
                            await SaveRefreshToken(authResult.RefreshToken, authFlow);
                    }
                }
            }

            return authData;
        }

        private string GetScopes(iManageAuthenticationFlow authFlow)
        {
            switch (authFlow)
            {
                case iManageAuthenticationFlow.Ropc:
                    return _iManageSettings.RopcFlow?.Scopes ?? "";

                case iManageAuthenticationFlow.AuthorizationCode:
                case iManageAuthenticationFlow.Pkce:
                    return _iManageSettings.AuthCodeFlow?.Scopes ?? "";
            }

            return string.Empty;
        }

        private async Task SaveRefreshToken(string refreshToken, iManageAuthenticationFlow authFlow)
        {
            var user = await _userManager.GetUserAsync(_user);

            if (user != null)
                await _userManager.SetAuthenticationTokenAsync(user, GetTokenProvider(authFlow), GetTokenName(), refreshToken);
        }

        private void SaveAuthData(AuthData authData, DateTime expires, iManageAuthenticationFlow authFlow)
        {
            _context.HttpContext?.Response.Cookies.Append(GetCookieName(authFlow), JsonSerializer.Serialize(authData), new CookieOptions()
            {
                Expires = expires,
                HttpOnly = true,
                Secure = true,
            });
        }

        private async Task<ApiData?> GetApiData(HttpClient client, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_iManageSettings.ServerUrl}/api");
            request.Headers.Add("X-Auth-Token", token);

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                var authData = JsonSerializer.Deserialize<AuthData>(result);

                return authData?.Data;
            }

            return null;
        }

        public iManageAuthenticationFlow GetAuthenticationFlow()
        {
            switch (_user.GetDocumentStorageAccountType())
            {
                case DocumentStorageAccountType.User:
                    return _iManageSettings.ClientAuthentication;

                case DocumentStorageAccountType.Service:
                    return _iManageSettings.ClientServiceAccountAuthentication;
            }

            //todo: readonly account ??
            return _iManageSettings.ClientServiceAccountAuthentication;
        }
    }

    public interface IiManageAuthProvider
    {
        Task<AuthData?> GetAuthDataByRopc();
        Task<AuthData?> GetAuthDataByAuthorizationCode(string code);
        Task<AuthData?> GetAuthDataByPkce(string code);

        Task<AuthData?> AcquireTokenSilent(iManageAuthenticationFlow authFlow);

        Task<string> Logout();

        Task<bool> IsTokenExpired(iManageAuthenticationFlow authFlow);

        iManageAuthenticationFlow GetAuthenticationFlow();
    }
}
