using Microsoft.Graph;
using R10.Core.Helpers;
using R10.Core.Identity;
using System.Security.Claims;

namespace R10.Web.Services.iManage
{
    public class iManageSettings
    {
        public string? ServerUrl { get; set; }

        public string? AuthorizationEndpoint { get; set; }

        public string? TokenEndpoint { get; set; }

        public string? LogoutEndpoint { get; set; }

        public AuthProviderSettings? AuthCodeFlow { get; set; }

        public AuthProviderSettings? RopcFlow { get; set; }

        public iManageAuthenticationFlow ClientAuthentication { get; set; } = iManageAuthenticationFlow.Pkce;

        public iManageAuthenticationFlow ClientServiceAccountAuthentication { get; set; } = iManageAuthenticationFlow.Ropc;

        public string? Library { get; set; }

        public string? DocumentLinkEndpoint { get; set; }

        public string? WorkspaceTemplateId { get; set; }

        public string? DefaultFolderName { get; set; }

        public WorkspaceCreation WorkspaceCreation { get; set; }

        public string GetDocumentLinkUrl()
        {
            if (string.IsNullOrEmpty(DocumentLinkEndpoint))
                return string.Empty;

            if (DocumentLinkEndpoint.StartsWith("http"))
                return DocumentLinkEndpoint;
            
            return $"{ServerUrl}{DocumentLinkEndpoint}";
        }
    }

    public class AuthProviderSettings
    {
        public string? ClientId { get; set; }

        public string? ClientSecret { get; set; } 

        public string? RedirectUri { get; set; } 

        public string? UserName { get; set; } //ROPC flow

        public string? Password { get; set; } //ROPC flow

        public string? Scopes { get; set; } //Scopes for getting auth code or id token
    }

    public enum iManageAuthenticationFlow
    {
        Ropc,
        AuthorizationCode,
        Pkce
    }

    public enum WorkspaceCreation
    {
        Disabled,   //CPI is not allowed to create workspaces
        Auto,       //Allow CPI to create workspaces automatically
        Manual      //Show new workspace button in iManage setup
    }
}
