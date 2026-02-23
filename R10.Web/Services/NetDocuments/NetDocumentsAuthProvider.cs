using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Web.Extensions;
using R10.Web.Models.NetDocumentsModels;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace R10.Web.Services.NetDocuments
{
    public interface  INetDocumentsAuthProvider
    {
        Task<AuthData?> GetAuthDataByClientCredentials();
        Task<AuthData?> GetAuthDataByAuthorizationCode(string code);
        Task<AuthData?> GetAuthDataByPkce(string code);

        Task<AuthData?> AcquireTokenSilent(NetDocumentsAuthenticationFlow authFlow);
        Task Logout();

        NetDocumentsAuthenticationFlow GetAuthenticationFlow();
    }

    public class NetDocumentsAuthProvider : INetDocumentsAuthProvider
    {
        private readonly IHttpContextAccessor _context;
        private readonly NetDocumentsSettings _netDocumentsSettings;
        private readonly UserManager<CPiUser> _userManager;
        private readonly ClaimsPrincipal _user;

        public NetDocumentsAuthProvider(
            IHttpContextAccessor context, 
            IOptions<NetDocumentsSettings> netDocumentsSettings, 
            UserManager<CPiUser> userManager, 
            ClaimsPrincipal user
            )
        {
            _context = context;
            _netDocumentsSettings = netDocumentsSettings.Value;
            _userManager = userManager;
            _user = user;
        }

        // Auth token cookie
        private string GetCookieName(NetDocumentsAuthenticationFlow authFlow) => $"CPI_NetDocuments_{_user.GetUserIdentifier()}_{(int)authFlow}";

        // Token provider and name for saving refresh token to tblCPiUserTokens
        private string GetTokenProvider(NetDocumentsAuthenticationFlow authFlow) => $"NetDocuments_{(int)authFlow}";
        private string GetTokenName() => $"refresh_token";

        private string BaseUrl => $"{_context.HttpContext?.Request.Scheme}://{_context.HttpContext?.Request.Host}{_context.HttpContext?.Request.PathBase}";
        private string TokenUrl => $"{_netDocumentsSettings.AccessTokenURL}";

        public async Task<AuthData?> GetAuthDataByAuthorizationCode(string code)
        {
            var authFlow = NetDocumentsAuthenticationFlow.AuthorizationCode;
            var redirectUri = $"{BaseUrl}{_netDocumentsSettings.AuthCodeFlow?.RedirectUri ?? ""}";

            var key = $"{_netDocumentsSettings.AuthCodeFlow?.ClientId ?? ""}:{_netDocumentsSettings.AuthCodeFlow?.ClientSecret ?? ""}";
            var basicAuth  = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key));
            var payload = new Dictionary<string, string>();

            payload.Add("redirect_uri", redirectUri);
            payload.Add("grant_type", "authorization_code");
            payload.Add("code", code);
            payload.Add("scope", _netDocumentsSettings.AuthCodeFlow?.Scopes ?? "");

            return await GetAuthData(authFlow, basicAuth, payload);
        }

        public async Task<AuthData?> GetAuthDataByPkce(string code)
        {
            var authFlow = NetDocumentsAuthenticationFlow.Pkce;
            var redirectUri = $"{BaseUrl}{_netDocumentsSettings.AuthCodeFlow?.RedirectUri ?? ""}";

            var key = $"{_netDocumentsSettings.AuthCodeFlow?.ClientId ?? ""}:{_netDocumentsSettings.AuthCodeFlow?.ClientSecret ?? ""}";
            var basicAuth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key));
            var payload = new Dictionary<string, string>();

            payload.Add("code_verifier", _context.HttpContext?.Session.GetString("code_verifier") ?? "");
            payload.Add("redirect_uri", redirectUri);
            payload.Add("grant_type", "authorization_code");
            payload.Add("code", code);
            payload.Add("scope", _netDocumentsSettings.AuthCodeFlow?.Scopes ?? "");

            return await GetAuthData(authFlow, basicAuth, payload);
        }

        public async Task<AuthData?> GetAuthDataByClientCredentials()
        {
            var authFlow = NetDocumentsAuthenticationFlow.ClientCredentials;
            var redirectUri = $"{BaseUrl}{_netDocumentsSettings.ClientCredentialsFlow?.RedirectUri ?? ""}";

            var key = $"{_netDocumentsSettings.ClientCredentialsFlow?.ClientId ?? ""}|{_netDocumentsSettings.Repository}:{_netDocumentsSettings.ClientCredentialsFlow?.ClientSecret ?? ""}";
            var basicAuth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key));
            var payload = new Dictionary<string, string>();

            payload.Add("grant_type", "client_credentials");
            payload.Add("scope", _netDocumentsSettings.ClientCredentialsFlow?.Scopes ?? "");

            return await GetAuthData(authFlow, basicAuth, payload);
        }

        private async Task<AuthData?> GetAuthData(NetDocumentsAuthenticationFlow authFlow, string basicAuth, Dictionary<string, string> payload)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl) { Content = new FormUrlEncodedContent(payload) };
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

                var requestStart = DateTime.Now;
                var response = await client.SendAsync(request);
                var result = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var authResult = JsonSerializer.Deserialize<AuthResult>(result);
                    var token = authResult?.AccessToken ?? "";
                    var userInfo = await GetUserInfo(client, token);
                    var authData = new AuthData(token, userInfo);

                    SaveAuthData(authData, requestStart.AddSeconds(int.Parse(authResult?.ExpiresIn ?? "0")), authFlow);

                    if (!string.IsNullOrEmpty(authResult?.RefreshToken))
                        await SaveRefreshToken(authResult.RefreshToken, authFlow);

                    return authData;
                }

                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);
            }
        }

        public async Task Logout()
        {
            var logoutUri = ""; //logout endpoint from app settings
            var client = new HttpClient();
            var authFlows = Enum.GetValues(typeof(NetDocumentsAuthenticationFlow)).Cast<NetDocumentsAuthenticationFlow>();
            var user = await _userManager.GetUserAsync(_user);

            foreach (var authFlow in authFlows)
            {
                // Call logout endpoint 
                if (!string.IsNullOrEmpty(logoutUri))
                {
                    var authData = GetTokenFromCache(authFlow);
                    if (authData != null)
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, logoutUri);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authData.AccessToken);
                        HttpResponseMessage response = await client.SendAsync(request);
                    }
                }

                // Delete cookies
                _context.HttpContext?.Response.Cookies.Delete(GetCookieName(authFlow));

                // Delete refresh tokens
                if (user != null)
                    await _userManager.RemoveAuthenticationTokenAsync(user, GetTokenProvider(authFlow), GetTokenName());
            }

            client.Dispose();
        }

        public async Task<AuthData?> AcquireTokenSilent(NetDocumentsAuthenticationFlow authFlow)
        {
            var authData = GetTokenFromCache(authFlow);

            if (authData == null)
            {
                var refreshToken = await GetRefreshToken(authFlow);

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var key = $"{_netDocumentsSettings.AuthCodeFlow?.ClientId ?? ""}:{_netDocumentsSettings.AuthCodeFlow?.ClientSecret ?? ""}";
                    var basicAuth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key));
                    var payload = new Dictionary<string, string>();

                    payload.Add("grant_type", "refresh_token");
                    payload.Add("refresh_token", refreshToken);

                    try
                    {
                        authData = await GetAuthData(authFlow, basicAuth, payload);
                    }
                    catch 
                    { 
                        // Refresh token has expired
                    }
                }
            }

            return authData;
        }

        public NetDocumentsAuthenticationFlow GetAuthenticationFlow()
        {
            switch (_user.GetDocumentStorageAccountType())
            {
                case DocumentStorageAccountType.User:
                    return _netDocumentsSettings.ClientAuthentication;

                case DocumentStorageAccountType.Service:
                    return _netDocumentsSettings.ClientServiceAccountAuthentication;
            }

            //todo: readonly account ??
            return _netDocumentsSettings.ClientServiceAccountAuthentication;
        }

        private AuthData? GetTokenFromCache(NetDocumentsAuthenticationFlow authFlow)
        {
            var cookie = _context.HttpContext?.Request.Cookies[GetCookieName(authFlow)];

            if (!string.IsNullOrEmpty(cookie))
                return JsonSerializer.Deserialize<AuthData>(cookie);

            return null;
        }

        private async Task<string?> GetRefreshToken(NetDocumentsAuthenticationFlow authFlow)
        {
            var refreshToken = "";
            var user = await _userManager.GetUserAsync(_user);

            if (user != null)
                refreshToken = await _userManager.GetAuthenticationTokenAsync(user, GetTokenProvider(authFlow), GetTokenName());

            return refreshToken;
        }

        private async Task SaveRefreshToken(string refreshToken, NetDocumentsAuthenticationFlow authFlow)
        {
            var user = await _userManager.GetUserAsync(_user);

            if (user != null)
                await _userManager.SetAuthenticationTokenAsync(user, GetTokenProvider(authFlow), GetTokenName(), refreshToken);
        }

        private void SaveAuthData(AuthData authData, DateTime expires, NetDocumentsAuthenticationFlow authFlow)
        {
            _context.HttpContext?.Response.Cookies.Append(GetCookieName(authFlow), JsonSerializer.Serialize(authData), new CookieOptions()
            {
                Expires = expires,
                HttpOnly = true,
                Secure = true,
            });
        }

        private async Task<UserInfo?> GetUserInfo(HttpClient client, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_netDocumentsSettings.BaseUrl}/v1/User/info");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("Accept", "application/json");

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                var authUserData = JsonSerializer.Deserialize<UserInfo>(result);

                return authUserData;
            }

            return null;
        }
    }
}
