using Microsoft.AspNetCore.Authorization;

namespace R10.Web.Security
{
    public class CPiServiceAccountRequirement : IAuthorizationRequirement
    {
        public string? ServiceAccountName { get; set; } // Service account user name
        public string? ClientType { get; set; } // Optional X-Client-Type request header

        public CPiServiceAccountRequirement(string? serviceAccountName, string? clientType = null)
        {
            ServiceAccountName = serviceAccountName;
            ClientType = clientType;
        }
    }
}
