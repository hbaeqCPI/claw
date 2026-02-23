using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Security
{
    public class AMSAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessAMS";
        public const string FullModify = "FullModifyAMS";
        public const string RemarksOnlyModify = "RemarksOnlyModifyAMS";
        public const string CanDelete = "CanDeleteAMS";
        public const string LimitedRead = "LimitedReadAMS";
        public const string FullRead = "FullReadAMS";
        public const string DecisionMaker = "DecisionMakerAMS";
        public const string RegularUser = "RegularUserAMS";
        public const string CanAccessPatent = "CanAccessPatentIntegrated";
        public const string Internal = "InternalAMS";
        public const string InternalFullModify = "InternalFullModifyAMS";

        public const string FullModifyByRespOffice = "FullModifyAMSByRespOffice";
        public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyAMSByRespOffice";
        public const string CanDeleteByRespOffice = "CanDeleteAMSByRespOffice";
        public const string LimitedReadByRespOffice = "LimitedReadAMSByRespOffice";
        public const string FullReadByRespOffice = "FullReadAMSByRespOffice";
        public const string DecisionMakerByRespOffice = "DecisionMakerAMSByRespOffice";

        public const string CanAccessAuxiliary = "CanAccessAMSAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyAMS";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyAMS";
        public const string AuxiliaryLimited = "AuxiliaryLimitedAMS";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeleteAMS";

        public const string CanAccessCustomQuery = "CanAccessAMSCustomQuery";
        public const string CustomQueryModify = "CustomQueryModifyAMS";

        //settings
        public const string IsStandalone = "IsAMSStandalone";
        public const string IsCorporation = "IsAMSCorporation";
        public const string IsLawFirm = "IsAMSLawFirm";
        public const string FullModifyLawFirm = "FullModifyAMSLawFirm";
        public const string CanAccessFeeSetup = "CanAccessAMSFeeSetup";
        public const string CanAccessVATRateSetup = "CanAccessAMSVATRateSetup";

        public const string CanAccessProducts = "CanAccessAMSProducts";
        public const string ProductsModify = "ProductsModifyAMS";
        public const string ProductsRemarksOnly = "ProductsRemarksOnlyAMS";
        public const string ProductsCanDelete = "ProductsCanDeleteAMS";

        public const string CanAccessDashboard = "CanAccessAMSDashboard";
        public const string CanAccessAudit = "CanAccessAuditAMS";

        public const string HasDecisionManagement = "HasAMSDecisionManagement";
    }
}
