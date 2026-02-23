using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Security
{
    public static class PatentAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessPatent";
        public const string FullModify = "FullModifyPatent";
        public const string RemarksOnlyModify = "RemarksOnlyModifyPatent";
        public const string CanDelete = "CanDeletePatent";
        public const string LimitedRead = "LimitedReadPatent";
        public const string CanUploadDocuments = "CanUploadDocumentsPatent";
        public const string FullRead = "FullReadPatent";
        public const string Internal = "InternalPatent";
        public const string InternalFullModify = "InternalFullModifyPatent";
        public const string InternalRTS = "InternalRTS";

        public const string FullModifyByRespOffice = "FullModifyPatentByRespOffice";
        public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyPatentByRespOffice";
        public const string CanDeleteByRespOffice = "CanDeletePatentByRespOffice";
        public const string LimitedReadByRespOffice = "LimitedReadPatentByRespOffice";
        public const string FullReadByRespOffice = "FullReadPatentByRespOffice";

        public const string CanAccessLetters = "CanAccessPatentLetters";
        public const string CanAccessLettersSetup = "CanAccessPatentLettersSetup";
        public const string LetterModify = "LetterModifyPatent";

        public const string CanAccessCustomQuery = "CanAccessPatentCustomQuery";
        public const string CustomQueryModify = "CustomQueryModifyPatent";

        public const string CanAccessAuxiliary = "CanAccessPatentAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyPatent";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyPatent";
        public const string AuxiliaryLimited = "AuxiliaryLimitedPatent";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeletePatent";

        public const string CanAccessCountryLaw = "CanAccessPatentCountryLaw";
        public const string CountryLawModify = "CountryLawModifyPatent";
        public const string CountryLawRemarksOnly = "CountryLawRemarksOnlyPatent";
        public const string CountryLawCanDelete = "CountryLawCanDeletePatent";

        public const string CanAccessActionType = "CanAccessPatentActionType";
        public const string ActionTypeModify = "ActionTypeModifyPatent";
        public const string ActionTypeRemarksOnly = "ActionTypeRemarksOnlyPatent";
        public const string ActionTypeCanDelete = "ActionTypeCanDeletePatent";

        public const string CanAccessAudit = "CanAccessAuditPatent";
        public const string CanAccessPortfolioOnboarding = "CanAccessPortfolioOnboardingPatent";
        public const string CanAccessInventorAward = "CanAccessInventorAward";
        public const string CanAccessInventorAwardAuxiliary = "CanAccessInventorAwardAuxiliary";
        public const string CanAccessRTS = "CanAccessRTS";
        public const string CanAccessCustomReport = "CanAccessCustomReportPatent";

        public const string CanAccessProducts = "CanAccessPatentProducts";
        public const string ProductsModify = "ProductsModifyPatent";
        public const string ProductsRemarksOnly = "ProductsRemarksOnlyPatent";
        public const string ProductsCanDelete = "ProductsCanDeletePatent";

        public const string CanAccessCostEstimator = "CanAccessPatentCostEstimator";
        public const string CostEstimatorModify = "CostEstimatorModifyPatent";
        public const string CostEstimatorRemarksOnly = "CostEstimatorRemarksOnlyPatent";
        public const string CostEstimatorCanDelete = "CostEstimatorCanDeletePatent";

        public const string CanAccessGermanRemuneration = "CanAccessGermanRemunerationPatent";
        public const string GermanRemunerationModify = "GermanRemunerationModifyPatent";
        public const string GermanRemunerationRemarksOnly = "GermanRemunerationRemarksOnlyPatent";
        public const string GermanRemunerationCanDelete = "GermanRemunerationCanDeletePatent";

        public const string CanAccessFrenchRemuneration = "CanAccessFrenchRemunerationPatent";
        public const string FrenchRemunerationModify = "FrenchRemunerationModifyPatent";
        public const string FrenchRemunerationRemarksOnly = "FrenchRemunerationRemarksOnlyPatent";
        public const string FrenchRemunerationCanDelete = "FrenchRemunerationCanDeletePatent";

        public const string CanAccessCostTracking = "CanAccessPatentCostTracking";
        public const string CostTrackingModify = "CostTrackingModifyPatent";
        public const string CostTrackingDelete = "CostTrackingDeletePatent";
        public const string CostTrackingModifyByRespOffice = "CostTrackingModifyPatentByRespOffice";
        public const string CostTrackingDeleteByRespOffice = "CostTrackingDeletePatentByRespOffice";
        public const string CostTrackingUpload = "CostTrackingUploadPatent";

        public const string PatentScoreModify = "PatentScoreModify";
        public const string CanAccessPatentScore = "CanAccessPatentScore";

        public const string CanReceiveActionDelegation = "CanReceiveActionDelegationPatent";

        public const string CanAccessDocumentVerification = "CanAccessPatentDocumentVerification";
        public const string DocumentVerificationModify = "DocumentVerificationModifyPatent";

        public const string CanAccessDashboard = "CanAccessPatentDashboard";

        public const string CanAccessTradeSecret = "CanAccessPatentTradeSecret";
        public const string CanAccessTradeSecretReports = "CanAccessPatentTradeSecretReports";

        public const string CanAccessWorkflow = "CanAccessPatentWorkflow";
        public const string WorkflowModify = "WorkflowModifyPatent";

        public const string CanAccessMainMenu = "CanAccessPatentMenu";

        public const string SoftDocketAdd = "SoftDocketAddPatent";
        public const string SoftDocketModify = "SoftDocketModifyPatent";

        public const string CanRequestDocket = "RequestDocketPatent";
    }
}
