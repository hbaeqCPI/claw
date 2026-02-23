namespace R10.Web.Services.NetDocuments
{
    public class NetDocumentsSettings
    {
        public string? BaseUrl { get; set; }

        public string? AuthorizationURL { get; set; }

        public string? AccessTokenURL { get; set; }

        public AuthProviderSettings? AuthCodeFlow { get; set; }

        public AuthProviderSettings? ClientCredentialsFlow { get; set; }

        public NetDocumentsAuthenticationFlow ClientAuthentication { get; set; } = NetDocumentsAuthenticationFlow.Pkce;
        public NetDocumentsAuthenticationFlow ClientServiceAccountAuthentication { get; set; } = NetDocumentsAuthenticationFlow.ClientCredentials;

        public string? Repository { get; set; }

        public string? Cabinet { get; set; }

        public WorkspaceCreation WorkspaceCreation { get; set; }

        public int WorkspaceClientAttributeId { get; set; }

        public int WorkspaceMatterAttributeId { get; set; }

        public string? DefaultFolderName { get; set; }

        public string? DocumentUrl { get; set; }

        public bool IsClientMatter => WorkspaceClientAttributeId > 0;
    }

    public class AuthProviderSettings
    {
        public string? ClientId { get; set; }

        public string? ClientSecret { get; set; }

        public string? RedirectUri { get; set; }

        public string? Scopes { get; set; }
    }

    public enum NetDocumentsAuthenticationFlow
    {
        ClientCredentials,
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
