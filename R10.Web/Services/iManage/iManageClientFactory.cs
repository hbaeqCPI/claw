using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Models.IManageModels;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace R10.Web.Services.iManage
{
    public class iManageClientFactory : IiManageClientFactory
    {
        private readonly IiManageAuthProvider _authProvider;
        private readonly iManageSettings _iManageSettings;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerService<ApiLog> _apiLogger;

        public iManageClientFactory(IiManageAuthProvider authProvider, IOptions<iManageSettings> iManageSettings, IHttpContextAccessor context, ILoggerService<ApiLog> apiLogger)
        {
            _authProvider = authProvider;
            _iManageSettings = iManageSettings.Value;
            _context = context;
            _apiLogger = apiLogger;
        }

        public async Task<iManageClient> GetClient()
        {
            return await GetClient(_authProvider.GetAuthenticationFlow());
        }

        public async Task<iManageClient?> GetClient(bool ignoreError = false)
        {
            try
            {
                return await GetClient(_authProvider.GetAuthenticationFlow());
            }
            catch (iManageServiceException ex)
            {
                if (ignoreError)
                    return null;

                throw ex;
            }
        }

        public async Task<iManageClient> GetServiceClient()
        {
            return await GetClient(_iManageSettings.ClientServiceAccountAuthentication);
        }

        private async Task<iManageClient> GetClient(iManageAuthenticationFlow authFlow)
        {
            AuthData? authData = await _authProvider.AcquireTokenSilent(authFlow);

            if (authData == null && authFlow == iManageAuthenticationFlow.Ropc)
                authData = await _authProvider.GetAuthDataByRopc();

            if (authData != null)
                return new iManageClient(_iManageSettings.Library ?? "", authData, _authProvider, _context, _apiLogger);

            throw new iManageServiceException("Error getting iManage work client.", HttpStatusCode.BadRequest);
        }
    }

    public interface IiManageClientFactory
    {
        Task<iManageClient> GetClient();

        /// <summary>
        /// Use in methods without iManageAuthorizationFilter.
        /// Returns null instead of throwing iManageServiceException.
        /// </summary>
        /// <param name="ignoreError"></param>
        /// <returns></returns>
        Task<iManageClient?> GetClient(bool ignoreError);

        Task<iManageClient> GetServiceClient();
    }

    public class iManageClient : HttpClient
    {
        const string iManage_RateLimitReset = "iManage.RateLimitReset";

        private readonly AuthData _authData;
        private readonly IiManageAuthProvider _authProvider;
        private readonly HttpContext? _httpContext;
        private readonly ILoggerService<ApiLog> _apiLogger;

        public string Library { get; }

        public iManageClient(string library, AuthData authData, IiManageAuthProvider authProvider, IHttpContextAccessor context, ILoggerService<ApiLog> apiLogger)
        {
            var customerId = authData.Data?.User?.CustomerId ?? 0;
            var apiUrl = authData.Data?.Versions?.OrderBy(d => d.Version).LastOrDefault()?.Url ?? ""; //https://cloudimanage.com/work/api/v2
            var customerEndpoint = $"/customers/{customerId}/"; //needs trailing slash for BaseAddress to work

            base.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            base.DefaultRequestHeaders.Add("X-Auth-Token", authData.XAuthToken);
            base.BaseAddress = new Uri($"{apiUrl}{customerEndpoint}");

            _authData = authData;
            _authProvider = authProvider;
            _httpContext = context.HttpContext;
            _apiLogger = apiLogger;

            Library = library;
        }

        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await SendAsync(request, CancellationToken.None);
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // block request if iManage rate limit is active
            if (!string.IsNullOrEmpty(_httpContext?.Request.Cookies[iManage_RateLimitReset]))
                throw new iManageServiceException("iManage Cloud services are unavailable.", HttpStatusCode.TooManyRequests);

            var response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        // delete auth cookies and clear cached tokens
                        await _authProvider.Logout();
                        break;

                    case HttpStatusCode.TooManyRequests:
                        // get retry-after header
                        var xRatelimitReset = response.Headers.TryGetValues("x-ratelimit-reset", out var xRatelimitResetHeader) ? xRatelimitResetHeader.FirstOrDefault() : "5";

                        // set retry-after cookie
                        if (_httpContext != null && !string.IsNullOrEmpty(xRatelimitReset))
                            _httpContext.Response.Cookies.Append(iManage_RateLimitReset, xRatelimitReset, new CookieOptions()
                            {
                                Path = _httpContext.Request.PathBase,
                                Expires = DateTime.Now.AddSeconds(Convert.ToInt32(xRatelimitReset)),
                                HttpOnly = true,
                                Secure = true
                            });
                        break;
                }
            }

            // log api call
            await _apiLogger.Add(new ApiLog()
            {
                Name = "iManage",
                RequestMethod = request.Method.Method,
                RequestUrl = request.RequestUri?.ToString() ?? "",
                StatusCode = ((int)response.StatusCode),
                UserId = _httpContext?.User.GetUserName() ?? "",
                TimeStamp = DateTime.Now
            });

            return response;
        }
    }
}
