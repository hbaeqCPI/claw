using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public class TrademarkAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessTrademark";
        public const string FullModify = "FullModifyTrademark";
        public const string RemarksOnlyModify = "RemarksOnlyModifyTrademark";
        public const string CanDelete = "CanDeleteTrademark";
        public const string LimitedRead = "LimitedReadTrademark";
        public const string FullRead = "FullReadTrademark";
        public const string CanUploadDocuments = "CanUploadDocumentsTrademark";
        public const string Internal = "InternalTrademark";
        public const string InternalFullModify = "InternalFullModifyTrademark";

        public const string FullModifyByRespOffice = "FullModifyTrademarkByRespOffice";
        public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyTrademarkByRespOffice";
        public const string CanDeleteByRespOffice = "CanDeleteTrademarkByRespOffice";
        public const string LimitedReadByRespOffice = "LimitedReadTrademarkByRespOffice";
        public const string FullReadByRespOffice = "FullReadTrademarkByRespOffice";

        public const string CanAccessLetters = "CanAccessTrademarkLetters";
        public const string CanAccessLettersSetup = "CanAccessTrademarkLettersSetup";
        public const string LetterModify = "LetterModifyTrademark";

        public const string CanAccessCustomQuery = "CanAccessTrademarkCustomQuery";
        public const string CustomQueryModify = "CustomQueryModifyTrademark";

        public const string CanAccessAuxiliary = "CanAccessTrademarkAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyTrademark";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyTrademark";
        public const string AuxiliaryLimited = "AuxiliaryLimitedTrademark";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeleteTrademark";

        public const string CanAccessCountryLaw = "CanAccessTrademarkCountryLaw";
        public const string CountryLawModify = "CountryLawModifyTrademark";
        public const string CountryLawRemarksOnly = "CountryLawRemarksOnlyTrademark";
        public const string CountryLawCanDelete = "CountryLawCanDeleteTrademark";

        public const string CanAccessActionType = "CanAccessTrademarkActionType";
        public const string ActionTypeModify = "ActionTypeModifyTrademark";
        public const string ActionTypeRemarksOnly = "ActionTypeRemarksOnlyTrademark";
        public const string ActionTypeCanDelete = "ActionTypeCanDeleteTrademark";

        public const string CanAccessAudit = "CanAccessAuditTrademark";
        public const string CanAccessPortfolioOnboarding = "CanAccessPortfolioOnboardingTrademark";
        public const string CanAccessTrademarkLinks = "CanAccessTrademarkLinks";
        public const string CanAccessCustomReport = "CanAccessCustomReportTrademark";

        public const string CanAccessProducts = "CanAccessTrademarkProducts";
        public const string ProductsModify = "ProductsModifyTrademark";
        public const string ProductsRemarksOnly = "ProductsRemarksOnlyTrademark";
        public const string ProductsCanDelete = "ProductsCanDeleteTrademark";

        public const string CanAccessCostTracking = "CanAccessTrademarkCostTracking";
        public const string CostTrackingModify = "CostTrackingModifyTrademark";
        public const string CostTrackingDelete = "CostTrackingDeleteTrademark";
        public const string CostTrackingModifyByRespOffice = "CostTrackingModifyTrademarkByRespOffice";
        public const string CostTrackingDeleteByRespOffice = "CostTrackingDeleteTrademarkByRespOffice";
        public const string CostTrackingUpload = "CostTrackingUploadTrademark";

        public const string CanReceiveActionDelegation = "CanReceiveActionDelegationTrademark";

        public const string CanAccessDocumentVerification = "CanAccessTrademarkDocumentVerification";
        public const string DocumentVerificationModify = "DocumentVerificationModifyTrademark";

        public const string CanAccessCostEstimator = "CanAccessTrademarkCostEstimator";
        public const string CostEstimatorModify = "CostEstimatorModifyTrademark";
        public const string CostEstimatorRemarksOnly = "CostEstimatorRemarksOnlyTrademark";
        public const string CostEstimatorCanDelete = "CostEstimatorCanDeleteTrademark";

        public const string CanAccessDashboard = "CanAccessTrademarkDashboard";

        public const string CanAccessWorkflow = "CanAccessTrademarkWorkflow";
        public const string WorkflowModify = "WorkflowModifyTrademark";

        public const string CanAccessMainMenu = "CanAccessTrademarkMenu";

        public const string SoftDocketAdd = "SoftDocketAddTrademark";
        public const string SoftDocketModify = "SoftDocketModifyTrademark";

        public const string CanRequestDocket = "RequestDocketTrademark";
    }
}
