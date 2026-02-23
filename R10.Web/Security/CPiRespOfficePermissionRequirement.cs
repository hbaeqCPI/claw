using Microsoft.AspNetCore.Authorization;
using R10.Core.Identity;

namespace R10.Web.Security
{
    public class CPiRespOfficePermissionRequirement : IAuthorizationRequirement
    {
        public string SystemType { get; set; }
        public List<string>? Roles { get; set; }
        public SystemStatusType? SystemStatus { get; set; }
        public bool ForbidAdmin { get; set; }

        public CPiRespOfficePermissionRequirement(string systemType, List<string>? roles = null, SystemStatusType? systemStatus = null, bool forbidAdmin = false)
        {
            SystemType = systemType;
            Roles = roles;
            SystemStatus = systemStatus;
            ForbidAdmin = forbidAdmin;
        }
    }
}
