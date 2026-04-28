using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public class GeneralMatterAuthorizationPolicy
    {         
        public const string CanAccessSystem = "CanAccessGeneralMatter";
        public const string FullModify = "FullModifyGeneralMatter";
        public const string RemarksOnlyModify = "RemarksOnlyModifyGeneralMatter";
        public const string CanDelete = "CanDeleteGeneralMatter";
        public const string LimitedRead = "LimitedReadGeneralMatter";
        public const string FullRead = "FullReadGeneralMatter";
        public const string CanUploadDocuments = "CanUploadDocumentsGeneralMatter";
        public const string Internal = "InternalGeneralMatter";
        public const string InternalFullModify = "InternalFullModifyGeneralMatter";

        public const string FullModifyByRespOffice = "FullModifyGeneralMatterByRespOffice";
        public const string RemarksOnlyModifyByRespOffice = "RemarksOnlyModifyGeneralMatterByRespOffice";
        public const string CanDeleteByRespOffice = "CanDeleteGeneralMatterByRespOffice";
        public const string LimitedReadByRespOffice = "LimitedReadGeneralMatterByRespOffice";
        public const string FullReadByRespOffice = "FullReadGeneralMatterByRespOffice";

        public const string CanAccessLetters = "CanAccessGeneralMattersLetters";
        public const string CanAccessLettersSetup = "CanAccessGeneralMattersLettersSetup";
        public const string LetterModify = "LetterModifyGeneralMatters";

        public const string CanAccessCustomQuery = "CanAccessGeneralMattersCustomQuery";
        public const string CustomQueryModify = "CustomQueryModifyGeneralMatters";

        public const string CanAccessAuxiliary = "CanAccessGeneralMattersAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyGeneralMatters";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyGeneralMatters";
        public const string AuxiliaryLimited = "AuxiliaryLimitedGeneralMatters";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeleteGeneralMatters";

        public const string CanAccessCountryLaw = "CanAccessGeneralMattersCountryLaw";
        public const string CountryLawModify = "CountryLawModifyGeneralMatters";
        public const string CountryLawRemarksOnly = "CountryLawRemarksOnlyGeneralMatters";
        public const string CountryLawCanDelete = "CountryLawCanDeleteGeneralMatters";

        public const string CanAccessActionType = "CanAccessGeneralMattersActionType";
        public const string ActionTypeModify = "ActionTypeModifyGeneralMatters";
        public const string ActionTypeRemarksOnly = "ActionTypeRemarksOnlyGeneralMatters";
        public const string ActionTypeCanDelete = "ActionTypeCanDeleteGeneralMatters";

        public const string CanAccessAudit = "CanAccessAuditGeneralMatter";
        public const string CanAccessPortfolioOnboarding = "CanAccessPortfolioOnboardingGeneralMatter";
        public const string CanAccessCustomReport = "CanAccessCustomReportGeneralMatter";

        public const string CanAccessProducts = "CanAccessGeneralMatterProducts";
        public const string ProductsModify = "ProductsModifyGeneralMatter";
        public const string ProductsRemarksOnly = "ProductsRemarksOnlyGeneralMatter";
        public const string ProductsCanDelete = "ProductsCanDeleteGeneralMatter";

        public const string CanAccessCostTracking = "CanAccessGeneralMatterCostTracking";
        public const string CostTrackingModify = "CostTrackingModifyGeneralMatter";
        public const string CostTrackingDelete = "CostTrackingDeleteGeneralMatter";
        public const string CostTrackingModifyByRespOffice = "CostTrackingModifyGeneralMatterByRespOffice";
        public const string CostTrackingDeleteByRespOffice = "CostTrackingDeleteGeneralMatterByRespOffice";
        public const string CostTrackingUpload = "CostTrackingUploadGeneralMatter";

        public const string CanReceiveActionDelegation = "CanReceiveActionDelegationGeneralMatter";

        public const string CanAccessDocumentVerification = "CanAccessGeneralMatterDocumentVerification";
        public const string DocumentVerificationModify = "DocumentVerificationModifyGeneralMatter";

        public const string CanAccessDashboard = "CanAccessGeneralMatterDashboard";

        public const string CanAccessWorkflow = "CanAccessGeneralMatterWorkflow";
        public const string WorkflowModify = "WorkflowModifyGeneralMatter";

        public const string SoftDocketAdd = "SoftDocketAddGeneralMatter";
        public const string SoftDocketModify = "SoftDocketModifyGeneralMatter";

        public const string CanRequestDocket = "RequestDocketGeneralMatter";
    }
}
