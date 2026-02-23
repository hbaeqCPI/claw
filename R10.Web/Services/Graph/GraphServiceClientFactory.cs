using Microsoft.Graph;
using System.Security;
using System.Net.Http.Headers;
using R10.Core.Helpers;
using Microsoft.Extensions.Options;
using R10.Core.Identity;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace R10.Web.Services
{
    public class GraphServiceClientFactory : IGraphServiceClientFactory
    {
        protected string OboIdTokenCacheKey = "GraphOboIdToken";

        protected readonly IGraphAuthProvider _authProvider;

        public GraphServiceClientFactory(IGraphAuthProvider authProvider)
        {
            _authProvider = authProvider;
        }

        public GraphServiceClient GetGraphClientByRopc(AuthProviderSettings authProviderSettings, string userName, SecureString password) =>
            new GraphServiceClient(new DelegateAuthenticationProvider(
                async requestMessage =>
                {
                    var result = await _authProvider.GetTokenByRopcAsync(authProviderSettings, userName, password);
                    if (result != null)
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                }));

        /// <summary>
        /// Auth provider is not suited for web apps
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public GraphServiceClient GetGraphClientInteractive(AuthProviderSettings authProviderSettings, string userId) =>
            new GraphServiceClient(new DelegateAuthenticationProvider(
                async requestMessage =>
                {
                    var result = await _authProvider.GetTokenInteractive(authProviderSettings, userId);
                    if (result != null)
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                }));

        public GraphServiceClient GetGraphClientOnBehalfOf(AuthProviderSettings authProviderSettings, string userId) =>
            new GraphServiceClient(new DelegateAuthenticationProvider(
                async requestMessage =>
                {
                    var result = await _authProvider.GetTokenOnBehalfOf(authProviderSettings, userId);
                    if (result != null)
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                }));

        public GraphServiceClient GetGraphClientByAuthorizationCode(AuthProviderSettings authProviderSettings, string userId) =>
            new GraphServiceClient(new DelegateAuthenticationProvider(
                async requestMessage =>
                {
                    var result = await _authProvider.GetTokenByAuthorizationCode(authProviderSettings, userId);
                    if (result != null)
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                }));

        /// <summary>
        /// Token from auth provider has no user context
        /// </summary>
        /// <returns></returns>
        public GraphServiceClient GetGraphClientByClientCredentials(AuthProviderSettings authProviderSettings) =>
            new GraphServiceClient(new DelegateAuthenticationProvider(
                async requestMessage =>
                {
                    var result = await _authProvider.GetTokenByClientCredentials(authProviderSettings);
                    if (result != null)
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                }));

        public async Task<bool> IsAuthenticated(AuthProviderSettings authProviderSettings, GraphClientAuthenticationFlow flow, string userId)
        {
            return await _authProvider.IsAuthenticated(authProviderSettings, flow, userId);
        }
    }

    public interface IGraphServiceClientFactory
    {
        GraphServiceClient GetGraphClientByRopc(AuthProviderSettings authProviderSettings,string userName, SecureString password);
        GraphServiceClient GetGraphClientInteractive(AuthProviderSettings authProviderSettings, string userId);
        GraphServiceClient GetGraphClientOnBehalfOf(AuthProviderSettings authProviderSettings, string userId);
        GraphServiceClient GetGraphClientByAuthorizationCode(AuthProviderSettings authProviderSettings, string userId);
        GraphServiceClient GetGraphClientByClientCredentials(AuthProviderSettings authProviderSettings);

        Task<bool> IsAuthenticated(AuthProviderSettings authProviderSettings, GraphClientAuthenticationFlow flow, string userId);
    }
}
