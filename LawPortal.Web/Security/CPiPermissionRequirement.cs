using Microsoft.AspNetCore.Authorization;
using LawPortal.Core.Identity;

namespace LawPortal.Web.Security
{
    public class CPiPermissionRequirement : IAuthorizationRequirement
    {
        public string SystemType { get; set; }
        public List<string>? Roles { get; set; }
        public SystemStatusType? SystemStatus { get; set; }
        public bool ForbidAdmin { get; set; }
        public List<CPiUserType> UserTypes { get; set; } = new List<CPiUserType>();

        public CPiPermissionRequirement(string systemType, List<string>? roles = null, SystemStatusType? systemStatus = null, bool forbidAdmin = false, List<CPiUserType>? userTypes = null)
        {
            SystemType = systemType;
            Roles = roles;
            SystemStatus = systemStatus;
            ForbidAdmin = forbidAdmin;
            UserTypes = userTypes ?? new List<CPiUserType>();
        }
    }
}
