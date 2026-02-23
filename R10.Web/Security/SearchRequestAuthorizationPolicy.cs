using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Security
{
    public class SearchRequestAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessSearchRequest";
        public const string FullModify = "FullModifySearchRequest";
        public const string RemarksOnlyModify = "RemarksOnlyModifySearchRequest";
        public const string CanDelete = "CanDeleteSearchRequest";
        public const string LimitedRead = "LimitedReadSearchRequest";
        public const string Reviewer = "ReviewerSearchRequest";
        //public const string Inventor = "InventorDMS";
        public const string RegularUser = "RegularUserSearchRequest";

        //public const string FullModifyByRespOffice = "FullModifyDMSByRespOffice";
        //public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyDMSByRespOffice";
        //public const string CanDeleteByRespOffice = "CanDeleteDMSByRespOffice";
        //public const string LimitedReadByRespOffice = "LimitedReadDMSByRespOffice";

        public const string CanAccessAuxiliary = "CanAccessSearchRequestAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyClearancce";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlySearchRequest";
        public const string AuxiliaryLimited = "AuxiliaryLimitedSearchRequest";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeleteSearchRequest";

        public const string CanAccessReview = "CanAccessSearchRequestReview";

        //public const string CanAccessDueDateList = "CanAccessDMSDueDateList";

        public const string CanAddClearance = "CanAddSearchRequest";

        public const string CanAccessDashboard = "CanAccessSearchRequestDashboard";
        public const string CanAccessAudit = "CanAccessAuditSearchRequest";

        public const string CanAccessWorkflow = "CanAccessSearchRequestWorkflow";
        public const string WorkflowModify = "WorkflowModifySearchRequest";
    }
}