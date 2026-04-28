using Microsoft.AspNetCore.Authorization;
using LawPortal.Core.Helpers;
using LawPortal.Core.Identity;

namespace LawPortal.Web.Security
{
    public class ModulePermissionRequirement : IAuthorizationRequirement
    {
        public CPiModule[] Modules { get; set; }
        public string? SystemType { get; set; }
        public List<string>? Roles { get; set; }
        public SystemStatusType? SystemStatus { get; set; }
        public bool ForbidAdmin { get; set; }
        public List<CPiUserType> UserTypes { get; set; } = new List<CPiUserType>();

        public ModulePermissionRequirement(CPiModule module, string? systemType, List<string>? roles = null, SystemStatusType? systemStatus = null, bool forbidAdmin = false, List<CPiUserType>? userTypes = null)
        {
            Modules = new CPiModule[] { module };
            SystemType = systemType;
            Roles = roles;
            SystemStatus = systemStatus;
            ForbidAdmin = forbidAdmin;
            UserTypes = userTypes ?? new List<CPiUserType>();
        }

        public ModulePermissionRequirement(List<string>? roles, params CPiModule[] modules)
        {
            Modules = modules;
            SystemType = null;
            Roles = roles;
            SystemStatus = null;
            ForbidAdmin = false;
        }

        public ModulePermissionRequirement(params CPiModule[] modules)
        {
            Modules = modules;
            SystemType = null;
            Roles = null;
            SystemStatus = null;
            ForbidAdmin = false;
        }
    }
}
