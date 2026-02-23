using R10.Core.Helpers;
using R10.Core.Identity;
using System.Security.Claims;

namespace R10.Web.Services
{
    public class GraphSettings
    {
        public AuthProviderSettings? Mail { get; set; }

        public AuthProviderSettings? SharePoint { get; set; }

        public List<MailSettings>? Mailboxes { get; set; }

        public SiteSettings? Site { get; set; }

        public bool HasMail {
            get => Mailboxes != null && Mailboxes.Any() && 
                (!string.IsNullOrEmpty(Mailboxes[0].User) || (Mailboxes[0].GraphClientAuthentication > 0));
        }

        public MailSettings? GetMailSettings(string? mailbox)
        {
            if (!string.IsNullOrEmpty(mailbox) && Mailboxes != null && Mailboxes.Any(m => m.MailboxName.ToLower() == mailbox.ToLower()))
                return Mailboxes.FirstOrDefault(m => m.MailboxName.ToLower() == mailbox.ToLower());

            return null;
        }

        public int GetMailboxId(string mailbox)
        {
            return Mailboxes == null ? 0 : Mailboxes.FindIndex(m => m.MailboxName.ToLower() == mailbox.ToLower());
        }
    }

    public class AuthProviderSettings
    {
        public string? ProviderName { get; set; }

        public string? TenantId { get; set; }

        public string? ClientId { get; set; }

        public string? ClientSecret { get; set; } //Flows using confidential app client (auth code, on behalf of, and client credentials)

        public string? Authority { get; set; } //On Behalf Of flow

        public string? AuthorizationEndpoint { get; set; } //Authorization Code flow

        public string? RedirectUri { get; set; } = "/graph/authorize"; //Authorization Code flow

        public bool AlwaysPromptUserConsent { get; set; } = true; //Authorization Code flow

        public string? Scopes { get; set; } //Scopes for getting auth code or id token
    }

    public class MailSettings
    {
        public string MailboxName { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public string[]? UnreadCountFolders { get; set; } //display names of main folders that will be included in total unread mail count
        public string[]? DenyChildFolders { get; set; } //display names of main folders that are not allowed to have child folders. existing child folders are hidden
        public MailDowloadSettings? Download { get; set; } = new MailDowloadSettings();
        public GraphClientAuthenticationFlow GraphClientAuthentication { get; set; } //for development only. production always set to ropc (0)
    }

    public class MailDowloadSettings
    {
        //do not set default string array
        //it's not replaced with appsettings values.
        //appsetting values are ADDED to the default array
        public string[]? CaseNumberCountrySubCaseSeparators { get; set; } //= new string[] { " - ", " -- ", " / ", " \\ ", "/", "\\", "-", "--", " " };
        public double IntervalInMinutes { get; set; } = 60;
        public int MaxCount { get; set; } = 1000; //maximum number of emails to process
    }

    public class SiteSettings
    {
        public string RelativePath { get; set; }
        public string HostName { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public GraphClientAuthenticationFlow GraphClientAuthentication { get; set; } = GraphClientAuthenticationFlow.AuthorizationCode;
        public GraphClientAuthenticationFlow GraphClientServiceAccountAuthentication { get; set; } = GraphClientAuthenticationFlow.ClientCredentials;

        public GraphClientAuthenticationFlow GetAuthenticationFlow(ClaimsPrincipal user)
        {
            switch(user.GetDocumentStorageAccountType())
            {
                case DocumentStorageAccountType.User:
                    return GraphClientAuthentication;

                case DocumentStorageAccountType.Service:
                    return GraphClientServiceAccountAuthentication;
            }

            //todo: readonly account ??
            return GraphClientServiceAccountAuthentication;
        }
    }

    public enum GraphClientAuthenticationFlow
    {
        Ropc,
        Interactive,
        OnBehalfOf,
        AuthorizationCode,
        ClientCredentials
    }
}
