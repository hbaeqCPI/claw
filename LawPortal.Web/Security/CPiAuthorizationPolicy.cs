using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public class CPiAuthorizationPolicy
    {
        public const string CPiAdmin = "CPiAdmin";
        public const string Administrator = "Administrator";
        public const string SystemUser = "SystemUser";  // User with assigned system role
        public const string DashboardUser = "DashboardUser"; // User with assigned system role or user with dashboard permission
        public const string ScheduledTask = "ScheduledTask"; // Task scheduler service account
    }
}
