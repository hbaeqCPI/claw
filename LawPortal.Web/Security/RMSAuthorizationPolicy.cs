using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public class RMSAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessRMS";
        public const string FullModify = "FullModifyRMS";
        public const string RemarksOnlyModify = "RemarksOnlyModifyRMS";
        public const string CanDelete = "CanDeleteRMS";
        public const string LimitedRead = "LimitedReadRMS";
        public const string FullRead = "FullReadRMS";
        public const string DecisionMaker = "DecisionMakerRMS";
        public const string RegularUser = "RegularUserRMS";
        public const string CanAccessTrademark = "CanAccessTrademarkIntegrated";

        public const string FullModifyByRespOffice = "FullModifyRMSByRespOffice";
        public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyRMSByRespOffice";
        public const string CanDeleteByRespOffice = "CanDeleteRMSByRespOffice";
        public const string LimitedReadByRespOffice = "LimitedReadRMSByRespOffice";
        public const string FullReadByRespOffice = "FullReadRMSByRespOffice";
        public const string DecisionMakerByRespOffice = "DecisionMakerRMSByRespOffice";

        public const string CanAccessAuxiliary = "CanAccessRMSAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyRMS";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyRMS";
        public const string AuxiliaryLimited = "AuxiliaryLimitedRMS";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeleteRMS";

        public const string CanAccessDashboard = "CanAccessRMSDashboard";
        public const string CanAccessAudit = "CanAccessAuditRMS";
    }
}
