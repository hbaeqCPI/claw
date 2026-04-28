using Microsoft.AspNetCore.Authorization;
using LawPortal.Core.Identity;

namespace LawPortal.Web.Security
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
