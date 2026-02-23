using Microsoft.AspNetCore.Authorization;
using R10.Core.Identity;

namespace R10.Web.Security
{
    public class SoftDocketPermissionRequirement : IAuthorizationRequirement
    {
        public string SystemType { get; set; }

        public SoftDocketPermissionRequirement(string systemType)
        {
            SystemType = systemType;
        }
    }
}
