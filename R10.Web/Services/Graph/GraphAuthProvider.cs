using Microsoft.Identity.Client;
using System.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http.Extensions;
using ActiveQueryBuilder.Web.Server.Models;
using System.Collections.Concurrent;

namespace R10.Web.Services
{
    /// <summary>
    /// Security token provider for Graph client
    /// https://learn.microsoft.com/en-us/graph/auth/
    /// </summary>
    public class GraphAuthProvider : IGraphAuthProvider
    {
        protected IPublicClientApplication? _publicApp;
        protected IConfidentialClientApplication? _confidentialApp;

        //The Dictionary<TKey, TValue> class supports multiple readers but doesn't support multiple writers:
        //https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-5.0#thread-safety
        //A thread-safe implementation of IDictionary<TKey, TValue> is the ConcurrentDictionary<TKey, TValue> class:
        //https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2?view=net-5.0
        //To test concurrency you may use the Parallel class:
        //https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.parallel?view=net-5.0
        //Parallel.For(0, 1000, i =>
        //{
        //     // dictionary.Add(...)
        //});
        //Using ConcurrentDictionary avoids the exception:
        //System.InvalidOperationException: Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.
        protected ConcurrentDictionary<string, IPublicClientApplication> _publicApps;
        protected ConcurrentDictionary<string, IConfidentialClientApplication> _confidentialApps;

        protected ConcurrentDictionary<string, string> _accountIds;

        protected readonly IMemoryCache _cache;

        protected readonly ILogger<GraphAuthProvider> _logger;

        public GraphAuthProvider(IMemoryCache cache, IHttpContextAccessor context, ILogger<GraphAuthProvider> logger)
        {
            _cache = cache;
            _publicApps = new ConcurrentDictionary<string, IPublicClientApplication>();
            _confidentialApps = new ConcurrentDictionary<string, IConfidentialClientApplication>();
            _accountIds = new ConcurrentDictionary<string, string>();
            _logger = logger;

            var request = context.HttpContext.Request;
            BaseUri = $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        protected string BaseUri { get; }

        protected IPublicClientApplication GetPublicApp(AuthProviderSettings authProviderSettings)
        {
            var cacheKey = authProviderSettings.TenantId + authProviderSettings.ClientId;

            if (!_publicApps.ContainsKey(cacheKey))
            {
                var publicApp = PublicClientApplicationBuilder.Create(authProviderSettings.ClientId)
                    .WithTenantId(authProviderSettings.TenantId)
                    //RedirectUri is needed when using AcquireTokenInteractive
                    //url: http://localhost (only loopback redirect uri is supported)
                    //type: InstalledClient
                    .WithRedirectUri("http://localhost")
                    //.WithDefaultRedirectUri()
                    .Build();

                _publicApps.AddOrUpdate(cacheKey, publicApp, (key, oldValue) => publicApp);
            }

            return _publicApps.FirstOrDefault(a => a.Key == cacheKey).Value;
        }

        protected IConfidentialClientApplication GetConfidentialApp(AuthProviderSettings authProviderSettings)
        {
            var cacheKey = authProviderSettings.TenantId + authProviderSettings.ClientId;

            if (!_confidentialApps.ContainsKey(cacheKey))
            {
                var confidentialApp = ConfidentialClientApplicationBuilder.Create(authProviderSettings.ClientId)
                        .WithClientSecret(authProviderSettings.ClientSecret)
                        .WithTenantId(authProviderSettings.TenantId)
                        //enable caching
                        //https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnet
                        .WithLegacyCacheCompatibility(false)
                        .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                        //authorization code flow redirect uri
                        //must match return uri of authorization code authority
                        .WithRedirectUri(BaseUri + authProviderSettings.RedirectUri)
                        .Build();

                _confidentialApps.AddOrUpdate(cacheKey, confidentialApp, (key, oldValue) => confidentialApp);
            }

            return _confidentialApps.FirstOrDefault(a => a.Key == cacheKey).Value;
        }

        protected string[] GetScopes(AuthProviderSettings authProviderSettings)
        {
            //authProviderSettings.Scopes is for getting authorization code or id token
            //return authProviderSettings.Scopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            //graph always use .default
            return new[] { "https://graph.microsoft.com/.default" };
        }

        /// <summary>
        /// Acquires a token from the token cache, or Username/password
        /// Note that using username/password is not recommended. See https://aka.ms/msal-net-up
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>An AuthenticationResult if the user successfully signed-in, or otherwise <c>null</c></returns>
        public async Task<AuthenticationResult?> GetTokenByRopcAsync(AuthProviderSettings authProviderSettings, string username, SecureString password)
        {
            AuthenticationResult? result = null;
            var app = GetPublicApp(authProviderSettings);
            var scopes = GetScopes(authProviderSettings);
            var accounts = await app.GetAccountsAsync();
            var accountKey = $"Graph_{authProviderSettings.ProviderName}_{GraphClientAuthenticationFlow.Ropc.ToString()}_{username}";
            
            if (accounts.Any())
            {
                try
                {
                    var accountId = _accountIds.GetValueOrDefault(accountKey);
                    var account = accounts.FirstOrDefault(a => a.HomeAccountId.Identifier == accountId);

                    // Attempt to get a token from the cache (or refresh it silently if needed)
                    result = await app.AcquireTokenSilent(scopes, account)
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // No token for the account. Will proceed below
                }
            }

            // Cache empty or no token for account in the cache, attempt by username/password
            if (result == null)
            {
                result = await AcquireTokenByRopcAsync(app, scopes, username, password);

                if (result != null)
                    _accountIds.AddOrUpdate(accountKey, result.Account.HomeAccountId.Identifier, (key, oldValue) => result.Account.HomeAccountId.Identifier);
            }

            return result;
        }

        /// <summary>
        /// Acquires a token from the token cache, or Interactive
        /// Only loopback redirect uri is supported (http://localhost)
        /// Not suited for web apps
        /// https://learn.microsoft.com/en-us/entra/msal/dotnet/
        /// https://learn.microsoft.com/en-us/graph/auth-v2-user
        /// https://learn.microsoft.com/en-us/entra/msal/dotnet/acquiring-tokens/desktop-mobile/acquiring-tokens-interactively
        /// https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-net-initializing-client-applications
        /// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-interactively
        /// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/MSAL.NET-uses-web-browser
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>An AuthenticationResult if the user successfully signed-in, or otherwise <c>null</c></returns>
        public async Task<AuthenticationResult?> GetTokenInteractive(AuthProviderSettings authProviderSettings, string userId)
        {
            AuthenticationResult? result = null;
            var app = GetPublicApp(authProviderSettings);
            var scopes = GetScopes(authProviderSettings);
            var accounts = await app.GetAccountsAsync();
            var accountKey = $"Graph_{authProviderSettings.ProviderName}_{GraphClientAuthenticationFlow.Interactive.ToString()}_{userId}";

            if (accounts.Any())
            {
                try
                {
                    var accountId = _accountIds.GetValueOrDefault(accountKey);
                    var account = accounts.FirstOrDefault(a => a.HomeAccountId.Identifier == accountId);

                    // Attempt to get a token from the cache (or refresh it silently if needed)
                    if (account != null)
                        result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // No token for the account. Will proceed below
                }
            }

            // Cache empty or no token for account in the cache, get token interactively
            if (result == null)
            {
                try
                {
                    result = await app.AcquireTokenInteractive(scopes)
                        //.NET Core does not support embedded browser
                        //https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/MSAL.NET-uses-web-browser
                        .WithUseEmbeddedWebView(false)
                        .WithSystemWebViewOptions(new SystemWebViewOptions()
                        {
                            HtmlMessageError = "<p> An error occured: {0}. Details {1}</p>",
                            //HtmlMessageSuccess = "<p> Mail authentication was successful.</p>",
                            //BrowserRedirectSuccess = new Uri("https://www.microsoft.com"),
                            //OpenBrowserAsync = SystemWebViewOptions.OpenWithEdgeBrowserAsync
                        })
                        .ExecuteAsync();

                    if (result != null)
                        _accountIds.AddOrUpdate(accountKey, result.Account.HomeAccountId.Identifier, (key, oldValue) => result.Account.HomeAccountId.Identifier);
                }
                catch (Exception e)
                {
                    var msg = e.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Acquires a token from the token cache, or OnBehalfOf 
        /// Id token needed for assertion is from cache 
        /// so it must be set prior to calling OnBehalfOf auth provider
        /// https://github.com/Azure-Samples/ms-identity-aspnet-webapi-onbehalfof
        /// https://learn.microsoft.com/en-us/graph/auth-v2-user?tabs=http
        /// https://learn.microsoft.com/en-us/entra/msal/dotnet/acquiring-tokens/web-apps-apis/on-behalf-of-flow
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>An AuthenticationResult if the user successfully signed-in, or otherwise <c>null</c></returns>
        public async Task<AuthenticationResult> GetTokenOnBehalfOf(AuthProviderSettings authProviderSettings, string userId)
        {
            var idToken = GetIdToken(authProviderSettings.ProviderName, GraphClientAuthenticationFlow.OnBehalfOf, userId);
            var userAssertion = new UserAssertion(idToken, "urn:ietf:params:oauth:grant-type:jwt-bearer");

            //test apps
            var app = GetConfidentialApp(authProviderSettings);
            var accounts = await app.GetAccountsAsync();

            //AcquireTokenOnBehalfOf handles caching
            //return await GetConfidentialApp(authProviderSettings).AcquireTokenOnBehalfOf(GetScopes(authProviderSettings), userAssertion).ExecuteAsync();
            return await app.AcquireTokenOnBehalfOf(GetScopes(authProviderSettings), userAssertion).ExecuteAsync();
        }

        /// <summary>
        /// Acquires a token from the token cache
        ///https://learn.microsoft.com/en-us/graph/auth-v2-user?tabs=http
        ///https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps
        /// </summary>
        /// <param name="authProviderSettings"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<AuthenticationResult> GetTokenByAuthorizationCode(AuthProviderSettings authProviderSettings, string userId)
        {
            AuthenticationResult? result = null;
            var app = GetConfidentialApp(authProviderSettings);
            var scopes = GetScopes(authProviderSettings);
            var accounts = await app.GetAccountsAsync();
            var accountKey = $"Graph_{authProviderSettings.ProviderName}_{GraphClientAuthenticationFlow.AuthorizationCode.ToString()}_{userId}";

           if (accounts.Any())
            {
                try
                {
                    var accountId = _accountIds.GetValueOrDefault(accountKey);
                    var account = accounts.FirstOrDefault(a => a.HomeAccountId.Identifier == accountId);

                    // Attempt to get a token from the cache (or refresh it silently if needed)
                    if (account != null)
                        result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // No token for the account. 
                }
            }

            return result;
        }

        /// <summary>
        /// Acquire token by auth code
        /// </summary>
        /// <param name="authProviderSettings"></param>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<AuthenticationResult> GetTokenByAuthorizationCode(AuthProviderSettings authProviderSettings, string userId, string code)
        {
            AuthenticationResult? result = null;
            var app = GetConfidentialApp(authProviderSettings);
            var scopes = GetScopes(authProviderSettings);
            var accountKey = $"Graph_{authProviderSettings.ProviderName}_{GraphClientAuthenticationFlow.AuthorizationCode.ToString()}_{userId}";

            try
            {
                result = await app.AcquireTokenByAuthorizationCode(scopes, code).ExecuteAsync();

                if (result != null)
                    _accountIds.AddOrUpdate(accountKey, result.Account.HomeAccountId.Identifier, (key, oldValue) => result.Account.HomeAccountId.Identifier);
                else
                    _logger.LogError("Unable to get Graph auth token for account key: {0}", accountKey);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return result;
        }

        public string? GetIdToken(string provider, GraphClientAuthenticationFlow flow, string userId)
        {
            return _cache.Get<string>($"GraphAuthToken_{provider}_{flow.ToString()}_{userId}");
        }

        public void SetIdToken(string provider, GraphClientAuthenticationFlow flow, string userId, string idToken, DateTimeOffset expiresOn)
        {
            _cache.Set<string>($"GraphAuthToken_{provider}_{flow.ToString()}_{userId}", idToken, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = expiresOn,
            });
        }

        public async Task<bool> IsAuthenticated(AuthProviderSettings authProviderSettings, GraphClientAuthenticationFlow flow, string userId)
        {
            switch (flow)
            {
                case GraphClientAuthenticationFlow.AuthorizationCode:
                    var accounts = await GetConfidentialApp(authProviderSettings).GetAccountsAsync();
                    var accountId = _accountIds.GetValueOrDefault($"Graph_{authProviderSettings.ProviderName}_{flow.ToString()}_{userId}");
                    
                    return accounts.Any(a => a.HomeAccountId.Identifier == accountId);

                case GraphClientAuthenticationFlow.OnBehalfOf:
                    return !string.IsNullOrEmpty(GetIdToken(authProviderSettings.ProviderName, flow, userId));
            }

            return true;
        }

        /// <summary>
        /// Acquires token without user context
        /// </summary>
        /// <returns></returns>
        public async Task<AuthenticationResult> GetTokenByClientCredentials(AuthProviderSettings authProviderSettings)
        {
            //AcquireTokenForClient handles caching
            var result = await GetConfidentialApp(authProviderSettings).AcquireTokenForClient(GetScopes(authProviderSettings)).ExecuteAsync(); ;
            return result;
        }

        private async Task<AuthenticationResult> AcquireTokenByRopcAsync(IPublicClientApplication app, string[] scopes, string username, SecureString password)
        {
            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenByUsernamePassword(scopes, username, password)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                // Here are the kind of error messages you could have, and possible mitigations

                // ------------------------------------------------------------------------
                // MsalUiRequiredException: 'AADSTS50055: Password is expired.
                // error:invalid_grant
                // suberror:user_password_expired
                // Mitigation: you need to have the user change their password first. This
                // requires an interaction with Azure AD, which is not possible with the username/password flow)
                // if you are not using .NET Core (which does not have any Web UI) by calling (once only) AcquireTokenAsync interactive. 
                // remember that Username/password is for public client applications that is desktop/mobile applications.
                // If you are using .NET core or don't want to call AcquireTokenAsync, you might want to:
                // - use device code flow (See https://aka.ms/msal-net-device-code-flow)
                // - or suggest the user to navigate to a URL to consent: https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&scope=user.read
                //   where the user will be prompted to change their password
                // ------------------------------------------------------------------------

                // ------------------------------------------------------------------------
                // MsalUiRequiredException: AADSTS65001: The user or administrator has not consented to use the application 
                // error:invalid_grant
                // suberror:consent_required
                // with ID '{appId}' named '{appName}'. Send an interactive authorization request for this user and resource.
                // Mitigation: you need to get user consent first. This can be done either statically (through the portal), or dynamically (but this
                // requires an interaction with Azure AD, which is not possible with the username/password flow)
                // Statically: in the portal by doing the following in the "API permissions" tab of the application registration: 
                // 1. Click "Add a permission" and add all the delegated permissions corresponding to the scopes you want (for instance
                // User.Read and User.ReadBasic.All)
                // 2. Click "Grant/revoke admin consent for <tenant>") and click "yes".
                // Dynamically, if you are not using .NET Core (which does not have any Web UI) by calling (once only) AcquireTokenAsync interactive. 
                // remember that Username/password is for public client applications that is desktop/mobile applications.
                // If you are using .NET core or don't want to call AcquireTokenAsync, you might want to:
                // - use device code flow (See https://aka.ms/msal-net-device-code-flow)
                // - or suggest the user to navigate to a URL to consent: https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&scope=user.read
                // ------------------------------------------------------------------------

                // ------------------------------------------------------------------------
                // ErrorCode: invalid_grant
                // SubError: basic_action
                // MsalUiRequiredException: AADSTS50079: The user is required to use multi-factor authentication.
                // The tenant admin for your organization has chosen to oblige users to perform multi-factor authentication. 
                // Mitigation: none for this flow
                // Your application cannot use the Username/Password grant. 
                // Like in the previous case, you might want to use an interactive flow (AcquireTokenAsync()), or Device Code Flow instead.
                // Note this is one of the reason why using username/password is not recommended;
                // ------------------------------------------------------------------------

                // ------------------------------------------------------------------------
                // ex.ErrorCode: invalid_grant
                // subError: null
                // Message = "AADSTS70002: Error validating credentials. AADSTS50126: Invalid username or password
                // In the case of a managed user (user from an Azure AD tenant opposed to a federated user, which would be owned
                // in another IdP through ADFS), the user has entered the wrong password
                // Mitigation: ask the user to re-enter the password
                // ------------------------------------------------------------------------

                // ------------------------------------------------------------------------
                // ex.ErrorCode: invalid_grant
                // subError: null
                // MsalServiceException: ADSTS50034: To sign into this application the account must be added to the {domainName} directory.
                // or The user account does not exist in the {domainName} directory. To sign into this application, the account must be added to the directory.
                // The user was not found in the directory
                // Explanation: wrong username
                // Mitigation: ask the user to re-enter the username. 
                // ------------------------------------------------------------------------
            }
            catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_request")
            {
                // ------------------------------------------------------------------------
                // AADSTS90010: The grant type is not supported over the /common or /consumers endpoints. Please use the /organizations or tenant-specific endpoint.
                // you used common.
                // Mitigation: as explained in the message from Azure AD, the authority you use in the application needs to be tenanted or otherwise "organizations". change the 
                // "Tenant": property in the appsettings.json to be a GUID (tenant Id), or domain name (contoso.com) if such a domain is registered with your tenant
                // or "organizations", if you want this application to sign-in users in any Work and School accounts.
                // ------------------------------------------------------------------------

            }
            catch (MsalServiceException ex) when (ex.ErrorCode == "unauthorized_client")
            {
                // ------------------------------------------------------------------------
                // AADSTS700016: Application with identifier '{clientId}' was not found in the directory '{domain}'.
                // This can happen if the application has not been installed by the administrator of the tenant or consented to by any user in the tenant. 
                // You may have sent your authentication request to the wrong tenant
                // Cause: The clientId in the appsettings.json might be wrong
                // Mitigation: check the clientId and the app registration
                // ------------------------------------------------------------------------
            }
            catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_client")
            {
                // ------------------------------------------------------------------------
                // AADSTS70002: The request body must contain the following parameter: 'client_secret or client_assertion'.
                // Explanation: this can happen if your application was not registered as a public client application in Azure AD 
                // Mitigation: in the Azure portal, edit the manifest for your application and set the `allowPublicClient` to `true` 
                // ------------------------------------------------------------------------
            }
            catch (MsalClientException ex) when (ex.ErrorCode == "unknown_user_type")
            {
                // Message = "Unsupported User Type 'Unknown'. Please see https://aka.ms/msal-net-up"
                // The user is not recognized as a managed user, or a federated user. Azure AD was not
                // able to identify the IdP that needs to process the user
                throw new ArgumentException("U/P: Wrong username", ex);
            }
            catch (MsalClientException ex) when (ex.ErrorCode == "user_realm_discovery_failed")
            {
                // The user is not recognized as a managed user, or a federated user. Azure AD was not
                // able to identify the IdP that needs to process the user. That's for instance the case
                // if you use a phone number
                throw new ArgumentException("U/P: Wrong username", ex);
            }
            catch (MsalClientException ex) when (ex.ErrorCode == "unknown_user")
            {
                // the username was probably empty
                // ex.Message = "Could not identify the user logged into the OS. See http://aka.ms/msal-net-iwa for details."
                throw new ArgumentException("U/P: Wrong username", ex);
            }
            catch (MsalClientException ex) when (ex.ErrorCode == "parsing_wstrust_response_failed")
            {
                // ------------------------------------------------------------------------
                // In the case of a Federated user (that is owned by a federated IdP, as opposed to a managed user owned in an Azure AD tenant) 
                // ID3242: The security token could not be authenticated or authorized.
                // The user does not exist or has entered the wrong password
                // ------------------------------------------------------------------------
            }
            return result;
        }
    }

    public interface IGraphAuthProvider
    {
        Task<AuthenticationResult> GetTokenByRopcAsync(AuthProviderSettings authProviderSettings, string username, SecureString password);
        Task<AuthenticationResult?> GetTokenInteractive(AuthProviderSettings authProviderSettings, string userId);
        Task<AuthenticationResult> GetTokenOnBehalfOf(AuthProviderSettings authProviderSettings, string userId);
        Task<AuthenticationResult> GetTokenByClientCredentials(AuthProviderSettings authProviderSettings);
        Task<AuthenticationResult> GetTokenByAuthorizationCode(AuthProviderSettings authProviderSettings, string userId);
        Task<AuthenticationResult> GetTokenByAuthorizationCode(AuthProviderSettings authProviderSettings, string userId, string code);

        //on behalf of flow id token caching
        string? GetIdToken(string provider, GraphClientAuthenticationFlow flow, string userId);
        void SetIdToken(string provider, GraphClientAuthenticationFlow flow, string userId, string idToken, DateTimeOffset expiresOn);

        Task<bool> IsAuthenticated(AuthProviderSettings authProviderSettings, GraphClientAuthenticationFlow flow, string userId);
    }
}
