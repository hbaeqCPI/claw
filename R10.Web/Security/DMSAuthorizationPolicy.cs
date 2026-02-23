using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Security
{
    public class DMSAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessDMS";
        public const string FullModify = "FullModifyDMS";
        public const string RemarksOnlyModify = "RemarksOnlyModifyDMS";
        public const string CanDelete = "CanDeleteDMS";
        public const string LimitedRead = "LimitedReadDMS";
        public const string Reviewer = "ReviewerDMS";
        public const string Previewer = "PreviewerDMS";
        public const string Inventor = "InventorDMS";
        public const string RegularUser = "RegularUserDMS";
        public const string CanUploadDocuments = "CanUploadDocumentsDMS";

        //public const string FullModifyByRespOffice = "FullModifyDMSByRespOffice";
        //public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyDMSByRespOffice";
        //public const string CanDeleteByRespOffice = "CanDeleteDMSByRespOffice";
        //public const string LimitedReadByRespOffice = "LimitedReadDMSByRespOffice";

        public const string CanAccessAuxiliary = "CanAccessDMSAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyDMS";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyDMS";
        public const string AuxiliaryLimited = "AuxiliaryLimitedDMS";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeleteDMS";

        public const string CanAccessReview = "CanAccessDMSReview";
        public const string CanAccessPreview = "CanAccessDMSPreview";

        public const string CanAccessDueDateList = "CanAccessDMSDueDateList";

        public const string CanAccessAudit = "CanAccessAuditDMS";

        public const string CanAddDisclosure = "CanAddDisclosure";

        public const string CanAccessDashboard = "CanAccessDMSDashboard";

        public const string CanAccessWorkflow = "CanAccessDMSWorkflow";
        public const string WorkflowModify = "WorkflowModifyDMS";

        public const string CanAccessTradeSecret = "CanAccessDMSTradeSecret";

        public const string CanAccessTradeSecretReports = "CanAccessDMSTradeSecretReports";
    }
}
