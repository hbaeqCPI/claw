using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public class PatentClearanceAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessPatClearance";
        public const string FullModify = "FullModifyPatClearance";
        public const string RemarksOnlyModify = "RemarksOnlyModifyPatClearance";
        public const string CanDelete = "CanDeletePatClearance";
        public const string LimitedRead = "LimitedReadPatClearance";
        public const string Reviewer = "ReviewerPatClearance";
        //public const string Inventor = "InventorDMS";
        public const string RegularUser = "RegularUserPatClearance";

        //public const string FullModifyByRespOffice = "FullModifyDMSByRespOffice";
        //public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyDMSByRespOffice";
        //public const string CanDeleteByRespOffice = "CanDeleteDMSByRespOffice";
        //public const string LimitedReadByRespOffice = "LimitedReadDMSByRespOffice";

        public const string CanAccessAuxiliary = "CanAccessPatClearanceAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyPatClearancce";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyPatClearance";
        public const string AuxiliaryLimited = "AuxiliaryLimitedPatClearance";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeletePatClearance";

        public const string CanAccessReview = "CanAccessPatClearanceReview";

        //public const string CanAccessDueDateList = "CanAccessDMSDueDateList";

        public const string CanAddClearance = "CanAddPatClearance";

        public const string CanAccessDashboard = "CanAccessPatClearanceDashboard";
        public const string CanAccessAudit = "CanAccessAuditPatClearance";

        public const string CanAccessWorkflow = "CanAccessPatClearanceWorkflow";
        public const string WorkflowModify = "WorkflowModifyPatClearance";
    }
}