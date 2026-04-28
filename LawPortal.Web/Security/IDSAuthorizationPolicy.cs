using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public static class IDSAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessIDS";
        public const string FullModify = "FullModifyIDS";

        public const string FullModifyByRespOffice = "FullModifyIDSByRespOffice";

        public const string CanAccessDashboard = "CanAccessIDSDashboard";

        public const string CanAccessIDSImport = "CanAccessIDSImport";
    }
}
