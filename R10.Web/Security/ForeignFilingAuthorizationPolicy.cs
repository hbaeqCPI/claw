using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Security
{
    public class ForeignFilingAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessForeignFiling";
        public const string FullModify = "FullModifyForeignFiling";
        public const string RemarksOnlyModify = "RemarksOnlyModifyForeignFiling";
        public const string CanDelete = "CanDeleteForeignFiling";
        public const string LimitedRead = "LimitedReadForeignFiling";
        public const string FullRead = "FullReadForeignFiling";
        public const string DecisionMaker = "DecisionMakerForeignFiling";
        public const string RegularUser = "RegularUserForeignFiling";
        public const string CanAccessPatent = "CanAccessPatentIntegrated";

        public const string FullModifyByRespOffice = "FullModifyForeignFilingByRespOffice";
        public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyForeignFilingByRespOffice";
        public const string CanDeleteByRespOffice = "CanDeleteForeignFilingByRespOffice";
        public const string LimitedReadByRespOffice = "LimitedReadForeignFilingByRespOffice";
        public const string FullReadByRespOffice = "FullReadForeignFilingByRespOffice";
        public const string DecisionMakerByRespOffice = "DecisionMakerForeignFilingByRespOffice";

        public const string CanAccessAuxiliary = "CanAccessForeignFilingAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyForeignFiling";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyForeignFiling";
        public const string AuxiliaryLimited = "AuxiliaryLimitedForeignFiling";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeleteForeignFiling";

        public const string CanAccessDashboard = "CanAccessForeignFilingDashboard";
        public const string CanAccessAudit = "CanAccessAuditForeignFiling";
    }
}
