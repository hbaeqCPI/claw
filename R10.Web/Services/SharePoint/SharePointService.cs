using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using R10.Core.Helpers;
using System.Security.Claims;

namespace R10.Web.Services.SharePoint
{
    public class SharePointService : ISharePointService
    {
        protected readonly ClaimsPrincipal _user;
        protected readonly IGraphServiceClientFactory _graphServiceClientFactory;
        protected readonly GraphSettings _graphSettings;

        public SharePointService(ClaimsPrincipal user, IGraphServiceClientFactory graphServiceClientFactory, IOptions<GraphSettings> graphSettings)
        {
            _user = user;
            _graphServiceClientFactory = graphServiceClientFactory;
            _graphSettings = graphSettings.Value;
        }

        public GraphServiceClient GetGraphClient()
        {
            switch (_graphSettings.Site.GetAuthenticationFlow(_user))
            {
                //Graph API delegated permissions need admin consent when using ropc flow
                //AD user's MFA must be disabled
                case GraphClientAuthenticationFlow.Ropc:
                    return _graphServiceClientFactory.GetGraphClientByRopc(_graphSettings.SharePoint, _graphSettings.Site.User, _graphSettings.Site.Password.ToSecureString());

                //authentication redirects to http://localhost which will have issues with hsts
                //hsts issue can be mitigated by deleting localhost in
                //chrome's domain security policies through chrome://net-internals/#hsts
                //designed for desktop and mobile apps
                //not suited for web apps
                case GraphClientAuthenticationFlow.Interactive:
                    return _graphServiceClientFactory.GetGraphClientInteractive(_graphSettings.SharePoint, _user.GetUserIdentifier());

                //must add oidc authentication middleware
                //must enable ID tokens (used for implicit and hybrid flows) in app registration
                case GraphClientAuthenticationFlow.OnBehalfOf:
                    return _graphServiceClientFactory.GetGraphClientOnBehalfOf(_graphSettings.SharePoint, _user.GetUserIdentifier());

                //authentication end point is a redirect so no need for middleware
                //msal handles token cache serialization
                case GraphClientAuthenticationFlow.AuthorizationCode:
                    return _graphServiceClientFactory.GetGraphClientByAuthorizationCode(_graphSettings.SharePoint, _user.GetUserIdentifier());

                //GraphClientAuthenticationFlow.ClientCredentials
                //Graph API application permissions need admin consent when using client credentials flow
                default:
                    return _graphServiceClientFactory.GetGraphClientByClientCredentials(_graphSettings.SharePoint);
            }
        }

        public GraphServiceClient GetGraphClientByClientCredentials()
        {
            //Ropc
            if (_graphSettings.Site.GraphClientServiceAccountAuthentication == GraphClientAuthenticationFlow.Ropc)
                return _graphServiceClientFactory.GetGraphClientByRopc(_graphSettings.SharePoint, _graphSettings.Site.User, _graphSettings.Site.Password.ToSecureString());

            //ClientCredentials
            return _graphServiceClientFactory.GetGraphClientByClientCredentials(_graphSettings.SharePoint);
        }
    }

    public interface ISharePointService
    {
        /// <summary>
        /// Builds a GraphServiceClient based on authorization provider settings in GraphSettings.SharePoint
        /// </summary>
        /// <returns>The built GraphServiceClient</returns>
        GraphServiceClient GetGraphClient();

        /// <summary>
        /// Builds a GraphServiceClient for background services based on authorization provider settings 
        /// in GraphSettings.Site.GraphClientServiceAccountAuthentication.
        /// Defaults to GraphClientAuthenticationFlow.ClientCredentials.
        /// </summary>
        /// <returns>The built GraphServiceClient</returns>
        GraphServiceClient GetGraphClientByClientCredentials();
    }
}
