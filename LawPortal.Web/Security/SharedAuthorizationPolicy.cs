using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public static class SharedAuthorizationPolicy
    {
        public const string CanAccessSystem = "CanAccessShared";
        public const string FullModify = "FullModifyShared";
        public const string RemarksOnlyModify = "RemarksOnlyModifyShared";
        public const string CanDelete = "CanDeleteShared";
        public const string LimitedRead = "LimitedReadShared";
        public const string FullRead = "FullReadShared";
        public const string Internal = "InternalShared";

        public const string DecisionMaker = "DecisionMakerShared"; //authorize if user has any DecisionMaker role regardless of system

        public const string CanUploadDocuments = "CanUploadDocumentsShared";

        public const string CanAccessLetters = "CanAccessLetters"; //authorize if user has any Letter roles regardless of system
        public const string CanAccessCustomQuery = "CanAccessCustomQuery"; //authorize if user has any CustomQuery roles regardless of system

        public const string CanAccessDueDateList = "CanAccessDueDateList"; //authorize if user has any CanAccessDueDateList roles regardless of system

        public const string CanAccessAudit = "CanAccessAudit";
        public const string CanAccessDeDocket = "CanAccessDeDocket";
        public const string CanAccessDeDocketAuxiliary = "CanAccessDeDocketAuxiliary";
        public const string CanAccessPortfolioOnboarding = "CanAccessPortfolioOnboarding";
        public const string CanAccessCustomReport = "CanAccessCustomReport";
        public const string CanAccessGlobalSearch = "CanAccessGlobalSearch";
        public const string CanAccessCustomFieldSetup = "CanAccessCustomFieldSetup";
        public const string CanAccessRecentViewed = "CanAccessRecentViewed";

        //settings
        public const string IsCorporation = "IsCorporation";
        public const string IsLawFirm = "IsLawFirm";

        //mail
        public const string CanAccessMail = "CanAccessMail";
        public static string GetMailboxPolicyName(string mailbox)
        {
            return $"CanAccessMail{mailbox.Replace(" ", string.Empty)}";
        }

        public const string CanAccessDashboard = "CanAccessSharedDashboard";
        public const string CanAccessPowerBIConnector = "CanAccessPowerBIConnector";

        public const string CanAccessDocumentVerification = "CanAccessDocumentVerification";

        public const string CanAccessESignature = "CanAccessESignature";
        public const string CanAccessESignatureAuxiliary = "CanAccessESignatureAuxiliary";

        public const string CanAccessTimeTrackerAuxiliary = "CanAccessTimeTrackerAuxiliary";        

        public const string TradeSecretAdmin = "TradeSecretAdmin";
        public const string CanAccessTradeSecret = "CanAccessTradeSecret";
    }
}
