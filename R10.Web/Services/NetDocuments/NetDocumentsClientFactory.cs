using Microsoft.Extensions.Options;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Models.NetDocumentsModels;
using System.Net;
using System.Net.Http.Headers;

namespace R10.Web.Services.NetDocuments
{
    public interface INetDocumentsClientFactory
    {
        Task<NetDocumentsClient> GetClient();

        /// <summary>
        /// Use in methods without NetDocumentsAuthorizationFilter.
        /// Returns null instead of throwing NetDocumentsServiceException.
        /// </summary>
        /// <param name="ignoreError"></param>
        /// <returns></returns>
        Task<NetDocumentsClient?> GetClient(bool ignoreError);

        Task<NetDocumentsClient> GetServiceClient();

    }

    public class NetDocumentsClientFactory : INetDocumentsClientFactory
    {
        private readonly INetDocumentsAuthProvider _authProvider;
        private readonly NetDocumentsSettings _netDocumentsSettings;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerService<ApiLog> _apiLogger;

        public NetDocumentsClientFactory(INetDocumentsAuthProvider authProvider, IOptions<NetDocumentsSettings> netDocumentsSettings, IHttpContextAccessor context, ILoggerService<ApiLog> apiLogger)
        {
            _authProvider = authProvider;
            _netDocumentsSettings = netDocumentsSettings.Value;
            _context = context;
            _apiLogger = apiLogger;
        }

        public async Task<NetDocumentsClient> GetClient()
        {
            return await GetClient(_authProvider.GetAuthenticationFlow());
        }

        public async Task<NetDocumentsClient?> GetClient(bool ignoreError)
        {
            try
            {
                return await GetClient(_authProvider.GetAuthenticationFlow());
            }
            catch (NetDocumentsServiceException ex)
            {
                if (ignoreError)
                    return null;

                throw ex;
            }
        }

        public async Task<NetDocumentsClient> GetServiceClient()
        {
            return await GetClient(_netDocumentsSettings.ClientServiceAccountAuthentication);
        }

        private async Task<NetDocumentsClient> GetClient(NetDocumentsAuthenticationFlow authFlow)
        {
            AuthData? authData = await _authProvider.AcquireTokenSilent(authFlow);

            if (authData == null && authFlow == NetDocumentsAuthenticationFlow.ClientCredentials)
                authData = await _authProvider.GetAuthDataByClientCredentials();

            if (authData != null)
                return new NetDocumentsClient(_netDocumentsSettings, authData, _authProvider, _context, _apiLogger);

            throw new NetDocumentsServiceException("Error getting NetDocuments client.", HttpStatusCode.BadRequest);
        }
    }

    public class NetDocumentsClient : HttpClient
    {
        const string netDocs_RetryAfter = "NetDocuments.RetryAfter";

        private readonly NetDocumentsSettings _netDocumentsSettings;
        private readonly AuthData _authData;
        private readonly INetDocumentsAuthProvider _authProvider;
        private readonly HttpContext? _httpContext;
        private readonly ILoggerService<ApiLog> _apiLogger;

        public string Repository => _netDocumentsSettings.Repository ?? "";
        public string Cabinet => _netDocumentsSettings.Cabinet ?? "";

        public NetDocumentsClient(NetDocumentsSettings netDocumentsSettings, AuthData authData, INetDocumentsAuthProvider authProvider, IHttpContextAccessor context, ILoggerService<ApiLog> apiLogger)
        {
            _netDocumentsSettings = netDocumentsSettings;
            _authData = authData;
            _authProvider = authProvider;
            _httpContext = context.HttpContext;
            _apiLogger = apiLogger;

            base.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            base.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData.AccessToken);
            base.DefaultRequestHeaders.Add("Accept", "application/json");
            base.BaseAddress = new Uri($"{_netDocumentsSettings.BaseUrl ?? ""}");
        }

        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await SendAsync(request, CancellationToken.None);
        }

        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // block request if NetDocs rate limit is active
            if (!string.IsNullOrEmpty(_httpContext?.Request.Cookies[netDocs_RetryAfter]))
                throw new NetDocumentsServiceException("NetDocuments services are unavailable.", HttpStatusCode.TooManyRequests);

            var response =  await base.SendAsync(request, cancellationToken);

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
                        var retryAfter = response.Headers.TryGetValues("Retry-After", out var retryAfterHeader) ? retryAfterHeader.FirstOrDefault() : "5";

                        // set retry-after cookie
                        if (_httpContext != null && !string.IsNullOrEmpty(retryAfter))
                            _httpContext.Response.Cookies.Append(netDocs_RetryAfter, retryAfter, new CookieOptions()
                            {
                                Path = _httpContext.Request.PathBase,
                                Expires = DateTime.Now.AddSeconds(Convert.ToInt32(retryAfter)),
                                HttpOnly = true,
                                Secure = true
                            });
                        break;
                }
            }

            // get header values
            IEnumerable<string>? headerValues;
            var apiAllowance = response.Headers.TryGetValues("NDRestAPIAllowance", out headerValues) ? headerValues.FirstOrDefault() : "";
            var apiAllowanceUsed = response.Headers.TryGetValues("NDRestAPIAllowanceUsed", out headerValues) ? headerValues.FirstOrDefault() : "";
            var apiCost = response.Headers.TryGetValues("NDRestAPICost", out headerValues) ? headerValues.FirstOrDefault() : "";

            // log api call
            await _apiLogger.Add(new ApiLog()
            {
                Name = "NetDocuments",
                RequestMethod = request.Method.Method,
                RequestUrl = request.RequestUri?.ToString() ?? "",
                StatusCode = ((int)response.StatusCode),
                UserId = _httpContext?.User.GetUserName() ?? "",
                TimeStamp = DateTime.Now,
                Allowance = int.Parse(string.IsNullOrEmpty(apiAllowance) ? "0" : apiAllowance),
                AllowanceUsed = int.Parse(string.IsNullOrEmpty(apiAllowanceUsed) ? "0" : apiAllowanceUsed),
                Cost = int.Parse(string.IsNullOrEmpty(apiCost) ? "0" : apiCost)
            });

            return response;
        }
    }
}
